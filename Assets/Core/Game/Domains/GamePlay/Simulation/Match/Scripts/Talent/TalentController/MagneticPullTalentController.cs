using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class MagneticPullTalentController : ITalentController
    {
        private ushort _casterPlayerId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;

        public TalentType TalentType => TalentType.MagneticPull;

        public MagneticPullTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (!wasTalentInputDownThisTick)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.MagneticPull, out int talentIndex))
            {
                LogService.LogError($"No MagneticPull talent found for player id {_casterPlayerId}");
                return;
            }

            var config = _gamePlayConfig.Talents.MagneticPullTalentConfig;
            var direction = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var offset = _sharedGamePlayConfig.MagneticPullFieldRadius*0.5f;
            var center = casterPlayerState.Spaceship.Transform.Position + (direction * offset);
            ushort hitEnemyId = 0;
            var didHitEnemy = _physicsSimulator.ArcCastOnPlayer(center, _sharedGamePlayConfig.MagneticPullFieldRadius,direction, _gamePlayConfig.Talents.MagneticPullTalentConfig.FieldArcAngle, (short)casterPlayerState.TeamId, out var hitBodyData);
            if (didHitEnemy)
            {
                var hitEnemyPlayer = _matchDataService.SimulationState.GetPlayerById(hitBodyData.Id);
                hitEnemyId = hitEnemyPlayer.Id;
                var force = config.PushForce;
                var dirToEnemy = Vector2.Normalize(hitEnemyPlayer.Spaceship.Transform.Position - casterPlayerState.Spaceship.Transform.Position);

                var forceToEnemy = -dirToEnemy * force;
                var forceToPlayer = dirToEnemy * force;
                hitEnemyPlayer.Spaceship.Transform.Velocity += forceToEnemy;
                casterPlayerState.Spaceship.Transform.Velocity += forceToPlayer;
            }

            // Put on cooldown
            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddCreateMagneticPullFieldNetEventS2C(tick, _casterPlayerId, direction, cooldownEndTick, didHitEnemy, hitEnemyId);
        }

        public void StopIfActive(int tick)
        {
            // Instant cast, nothing to stop
        }

        public void OnTick(int tick, float deltaTime)
        {
            // Instant cast, nothing to update
        }

        public void ResetData()
        {
        }
    }
}
