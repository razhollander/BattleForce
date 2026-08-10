using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class SoulTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private ushort _ghostId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedConfig;
        private readonly IScoreGatePassTrackerService _scoreGatePassTrackerService;

        public TalentType TalentType => TalentType.Soul;

        private bool IsCurrentlyActive
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        public SoulTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedConfig, IScoreGatePassTrackerService scoreGatePassTrackerService)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedConfig = sharedConfig;
            _scoreGatePassTrackerService = scoreGatePassTrackerService;
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

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!IsCurrentlyActive)
            {
                if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
                {
                    return;
                }

                ActivateTalent(tick, casterPlayerState);
            }
            else
            {
                TeleportToGhost(tick, casterPlayerState);
            }
        }

        private void ActivateTalent(int tick, PlayerStateS2C casterPlayerState)
        {
            IsCurrentlyActive = true;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.SoulTalentConfig;
            var direction = casterPlayerState.Spaceship.Transform.Direction;
            var velocity = direction * config.GhostSpeed;
            var position = casterPlayerState.Spaceship.Transform.Position + direction * config.SpawnForwardOffset;
            var size = _sharedConfig.SoulGhostSize;

            var ghost = _matchDataService.AddSoulGhost(_casterPlayerId, position, direction, velocity);
            _ghostId = ghost.Id;
            _physicsSimulator.AddSoulGhost(_ghostId, casterPlayerState.TeamId, ghost.Position, size, velocity);
            _netEventsDataService.AddCreateSoulGhostNetEvent(tick, _ghostId, _casterPlayerId, ghost.Position, direction);
        }

        private void TeleportToGhost(int tick, PlayerStateS2C casterPlayerState)
        {
            ref var ghost = ref _matchDataService.SimulationState.GetSoulGhostById(_ghostId);
            var ghostPosition = ghost.Position;
            var ghostDirection = ghost.Direction;

            casterPlayerState.Spaceship.Transform.Position = ghostPosition;
            casterPlayerState.Spaceship.Transform.Direction = ghostDirection;
            casterPlayerState.Spaceship.Transform.Velocity = Vector2.Zero;
            _scoreGatePassTrackerService.InvalidatePreviousPosition(_casterPlayerId); // the jump to the ghost must not be read as a gate pass

            DeactivateTalent(tick, didTeleport: true, ghostPosition, ghostDirection);
        }

        public void HitWall(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            if (!_matchDataService.SimulationState.TryGetSoulGhostById(_ghostId, out _))
            {
                return;
            }

            DeactivateTalent(tick, didTeleport: false, Vector2.Zero, Vector2.Zero);
        }

        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            DeactivateTalent(tick, didTeleport: false, Vector2.Zero, Vector2.Zero);
        }

        public void OnTick(int tick, float deltaTime)
        {
            // The ghost flies straight at a constant velocity; physics moves it and StepPhysiscsSimulationCommand syncs its position.
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            _ghostId = 0;
        }

        private void DeactivateTalent(int tick, bool didTeleport, Vector2 teleportPosition, Vector2 teleportDirection)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            int cooldownEndTick = tick;
            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Soul, out int talentIndex))
            {
                LogService.LogError($"No Soul talent found for player id {_casterPlayerId}");
            }
            else
            {
                ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
                cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
                talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;
            }

            _physicsSimulator.RemoveSoulGhost(_ghostId);
            _matchDataService.SimulationState.RemoveSoulGhostById(_ghostId);
            _netEventsDataService.AddDeactivateSoulTalentNetEvent(tick, _ghostId, _casterPlayerId, cooldownEndTick, didTeleport, teleportPosition, teleportDirection);
        }
    }
}
