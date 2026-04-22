using Box2D.NetStandard.Dynamics.Bodies;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class RockTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private int _startTick;
        private float _originalRadius;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IPhysicsSimulator _physicsSimulator;

        public TalentType TalentType => TalentType.Rock;

        private bool IsCurrentlyActive
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        public RockTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, IPhysicsSimulator physicsSimulator)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _physicsSimulator = physicsSimulator;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive || !wasTalentInputDownThisTick)
            {
                if (wasTalentInputDownThisTick && IsCurrentlyActive)
                {
                    DeactivateTalent(tick);
                }
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isOnCooldown = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown();

            if (isOnCooldown)
            {
                return;
            }

            IsCurrentlyActive = true;
            _startTick = tick;

            _originalRadius = casterPlayerState.Spaceship.Transform.Radius;
            casterPlayerState.Spaceship.Transform.Radius *= _gamePlayConfig.Talents.RockTalentConfig.SizeMultiplier;
            casterPlayerState.Spaceship.IsEngineOn = false;
            casterPlayerState.Spaceship.Transform.StopMotion();

            // Disable player body, and add a wall over him to block things.
            var playerBody = _physicsSimulator.GetPlayer(_casterPlayerId);
            playerBody.SetEnabled(false);

            _physicsSimulator.AddRockWall(_casterPlayerId, casterPlayerState.Spaceship.Transform.Position, casterPlayerState.Spaceship.Transform.Radius);

            _netEventsDataService.AddActivateRockTalentNetEvent(tick, _casterPlayerId);
        }

        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive) return;
            DeactivateTalent(tick);
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive) return;

            var elapsedSeconds = (tick - _startTick) * deltaTime;
            var didTimeEnd = elapsedSeconds >= _gamePlayConfig.Talents.RockTalentConfig.DurationInSeconds;

            if (didTimeEnd)
            {
                DeactivateTalent(tick);
            }
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (casterPlayerState.Spaceship.IsAlive)
            {
                casterPlayerState.Spaceship.IsEngineOn = true;
            }

            casterPlayerState.Spaceship.Transform.Radius = _originalRadius;

            var playerBody = _physicsSimulator.GetPlayer(_casterPlayerId);
            playerBody.SetEnabled(true);

            _physicsSimulator.RemoveRockWall(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Rock, out int talentIndex))
            {
                LogService.LogError($"No Rock talent found for player id {_casterPlayerId}");
                return;
            }

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddDeactivateRockTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            _originalRadius = 0;
        }
    }
}
