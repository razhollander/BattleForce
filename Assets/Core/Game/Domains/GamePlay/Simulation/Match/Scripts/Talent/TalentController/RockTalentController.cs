using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class RockTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private int _startTick;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly ICommandFactory _commandFactory;
        private readonly IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;
        private TrySpinPlayerCommand _trySpinPlayerCommand;
        private UpdatePlayerLavaExposureCommand _updatePlayerLavaExposureCommand;

        public TalentType TalentType => TalentType.Rock;

        private bool IsCurrentlyActive
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        public RockTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, ICommandFactory commandFactory, IPlayersInLavaTrackerService playersInLavaTrackerService)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _commandFactory = commandFactory;
            _playersInLavaTrackerService = playersInLavaTrackerService;
        }

        public void InitEntryPoint()
        {
            _tryAddForceToPlayerCommand = _commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
            _trySpinPlayerCommand = _commandFactory.CreateCommandVoid<TrySpinPlayerCommand>();
            _updatePlayerLavaExposureCommand = _commandFactory.CreateCommandVoid<UpdatePlayerLavaExposureCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            if (!wasTalentInputDownThisTick)
            {
                return;
            }

            if (IsCurrentlyActive)
            {
                DeactivateTalent(tick);
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            ActivateTalent(tick, casterPlayerState);
        }

        private void ActivateTalent(int tick, PlayerStateS2C casterPlayerState)
        {
            IsCurrentlyActive = true;
            _startTick = tick;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.RockTalentConfig;
            var casterSpaceship = casterPlayerState.Spaceship;
            casterSpaceship.IsEngineOn = false;
            casterSpaceship.Transform.StopMotion();

            _physicsSimulator.EnableRockBody(_casterPlayerId, config.ColliderRadiusMultiplier, config.BodyDensity, config.Restitution);
            _physicsSimulator.DisablePlayerHeartCollider(_casterPlayerId);

            PushAndSpinNearbyEnemies(tick, casterPlayerState, config);
            casterSpaceship.IsSpinned = false;
            _netEventsDataService.AddPlayerSpinnedEndedNetEvent(tick, _casterPlayerId);
            _netEventsDataService.AddActivateRockTalentNetEvent(tick, _casterPlayerId);
            _updatePlayerLavaExposureCommand.SetPlayerId(_casterPlayerId).SetProcessedTick(tick).Execute();
        }

        private void PushAndSpinNearbyEnemies(int tick, PlayerStateS2C casterPlayerState, RockTalentConfig config)
        {
            var rockPosition = casterPlayerState.Spaceship.Transform.Position;
            var radiusSquared = config.EnemyPushRadius * config.EnemyPushRadius;

            foreach (var enemyPlayerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var isSelf = enemyPlayerState.Id == _casterPlayerId;
                var isSameTeam = enemyPlayerState.TeamId == casterPlayerState.TeamId;
                if (isSelf || isSameTeam || !enemyPlayerState.Spaceship.IsAlive)
                {
                    continue;
                }

                var toEnemy = enemyPlayerState.Spaceship.Transform.Position - rockPosition;
                if (toEnemy.LengthSquared() > radiusSquared)
                {
                    continue;
                }

                var pushDirection = toEnemy.NormalizeSafe();
                _tryAddForceToPlayerCommand.SetPlayerId(enemyPlayerState.Id).SetForce(pushDirection * config.EnemyPushForce).ShouldTurnOffEngine(true).Execute();
                _trySpinPlayerCommand.SetPlayer(enemyPlayerState.Id).SetSpinAmount(config.EnemySpinAmount).SetTick(tick).Execute();
            }
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            var elapsedSecondsBeingRock = (tick - _startTick) * deltaTime;
            var didRockDurationFinish = elapsedSecondsBeingRock >= _gamePlayConfigService.GamePlayConfig.Talents.RockTalentConfig.DurationInSeconds;
            if (didRockDurationFinish)
            {
                DeactivateTalent(tick);
            }
        }

        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            DeactivateTalent(tick);
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            _physicsSimulator.DisableRockBody(_casterPlayerId, casterPlayerState.Spaceship.Transform.Radius, casterPlayerState.TeamId);
            _physicsSimulator.EnablePlayerHeartCollider(_casterPlayerId);

            if (casterPlayerState.Spaceship.IsAlive)
            {
                casterPlayerState.Spaceship.IsEngineOn = true;
            }

            int cooldownEndTick = tick;
            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Rock, out int talentIndex))
            {
                LogService.LogError($"No Rock talent found for player id {_casterPlayerId}");
            }
            else
            {
                ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
                cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
                talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;
            }

            _netEventsDataService.AddDeactivateRockTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
            _playersInLavaTrackerService.TryResetPlayerTimePassedSinceLastDamageTaken(_casterPlayerId);

            // Immunity ends with Rock: if the player is still in lava, resume the exposed state.
            _updatePlayerLavaExposureCommand.SetPlayerId(_casterPlayerId).SetProcessedTick(tick).Execute();
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
