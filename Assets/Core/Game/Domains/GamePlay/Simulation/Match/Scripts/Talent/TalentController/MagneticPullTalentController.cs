using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
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
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class MagneticPullTalentController : ITalentController
    {
        private ushort _casterPlayerId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly SpinPlayerCommand _spinPlayerCommand;

        public TalentType TalentType => TalentType.MagneticPull;

        private bool IsCurrentlyAiming
        {
            get
            {
                return _matchDataService.SimulationState.GetIsTalentAimingForPlayer(_casterPlayerId, TalentType);
            }
            set
            {
                _matchDataService.SimulationState.SetIsTalentCurrentlyAimingForPlayer(_casterPlayerId, TalentType, value);
            }
        }
        
        public MagneticPullTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _spinPlayerCommand = commandFactory.CreateCommandVoid<SpinPlayerCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            var isCurrentlyAiming = IsCurrentlyAiming;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isCurrentSelectedTalentOnCooldown = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown();
            if (isCurrentSelectedTalentOnCooldown)
            {
                return;
            }

            if (wasTalentInputDownThisTick)
            {
                if (!isCurrentlyAiming)
                {
                    IsCurrentlyAiming = true;
                    casterPlayerState.Spaceship.AssistArrowType = Shared.Scripts.Enums.PlayerAssistArrowType.AimArrow;
                }
            }

            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }

            casterPlayerState.Spaceship.AssistArrowType = Shared.Scripts.Enums.PlayerAssistArrowType.Hidden;
            IsCurrentlyAiming = false;

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType, out int talentIndex))
            {
                LogService.LogError($"No MagneticPull talent found for player id {_casterPlayerId}");
                return;
            }
            
            var direction = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var offset = casterPlayerState.Spaceship.Transform.Radius;
            var center = casterPlayerState.Spaceship.Transform.Position + (direction * offset);
            ushort hitEnemyId = 0;

            var didHitEnemy = _physicsSimulator.ArcCastOnPlayers(center, _sharedGamePlayConfig.MagneticPullFieldRadius, direction,
                _gamePlayConfigService.GamePlayConfig.Talents.MagneticPullTalentConfig.FieldArcAngle, (short) casterPlayerState.TeamId, out var hitBodyData);
            if (didHitEnemy)
            {
                hitEnemyId = hitBodyData.Id;
                ApplyPullToEnemyPhysics(tick, hitEnemyId, casterPlayerState);
            }
            
            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown * casterPlayerState.Spaceship.TalentsState.AllTalentsCooldownMultiplier, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddCreateMagneticPullFieldNetEventS2C(tick, _casterPlayerId, center, direction, cooldownEndTick, didHitEnemy, hitEnemyId);
        }

        private void ApplyPullToEnemyPhysics(int tick, ushort enemyId, PlayerStateS2C casterPlayerState)
        {
            var config = _gamePlayConfigService.GamePlayConfig.Talents.MagneticPullTalentConfig;
            var hitEnemyPlayer = _matchDataService.SimulationState.GetPlayerById(enemyId);
            var pullForce = config.PushForce;
            var directionToEnemy = Vector2.Normalize(hitEnemyPlayer.Spaceship.Transform.Position - casterPlayerState.Spaceship.Transform.Position);

            var forceToEnemy = -directionToEnemy * pullForce;
            hitEnemyPlayer.Spaceship.Transform.Velocity += forceToEnemy;
            
            //var forceToPlayer = directionToEnemy * pullForce;
            //casterPlayerState.Spaceship.Transform.Velocity += forceToPlayer;
                
            var randomSpin = RNG.NextFloat(config.MinSpin, config.MaxSpin);
            _spinPlayerCommand.SetPlayer(hitEnemyPlayer.Id).SetSpinAmount(randomSpin).SetTick(tick).Execute();
        }

        public void StopIfActive(int tick)
        {
            
        }

        public void OnTick(int tick, float deltaTime)
        {
            
        }

        public void ResetData()
        {
        }
    }
}
