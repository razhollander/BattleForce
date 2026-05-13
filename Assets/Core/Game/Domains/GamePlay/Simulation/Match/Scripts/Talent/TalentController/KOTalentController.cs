using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class KOTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private ushort _projectileId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SpinPlayerCommand _spinPlayerCommand;

        public TalentType TalentType => TalentType.KO;
        private bool IsCurrentlyActive
        {
            get
            {
                return _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            }
            set
            {
                _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
            }
        }
        
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
        
        private bool _isInReturnPhase;

        public KOTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
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
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            if (wasTalentInputDownThisTick)
            {
                if (!IsCurrentlyActive && !isCurrentlyAiming)
                {
                    IsCurrentlyAiming = true;
                    casterPlayerState.Spaceship.AssistArrowType = Core.Game.Domains.GamePlay.Shared.Scripts.Enums.PlayerAssistArrowType.AimArrow;
                }
            }

            if (IsCurrentlyActive)
            {
                return;
            }

            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }

            casterPlayerState.Spaceship.AssistArrowType = Core.Game.Domains.GamePlay.Shared.Scripts.Enums.PlayerAssistArrowType.Hidden;
            IsCurrentlyAiming = false;
            IsCurrentlyActive = true;
            _isInReturnPhase = false;

            var koConfig = _gamePlayConfig.Talents.KOTalentConfig;
            var aimDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var velocity = aimDirection * koConfig.ProjectileSpeed;
            var koProjectile = _matchDataService.AddKOProjectile(tick, _casterPlayerId, casterPlayerState.Spaceship.Transform.Position, aimDirection, velocity, koConfig.ProjectileSize);
            _projectileId = koProjectile.Id;
            _physicsSimulator.AddKOProjectile(_projectileId, casterPlayerState.TeamId, koProjectile.Position, koConfig.ProjectileSize, velocity);
            _netEventsDataService.AddCreateKOProjectileNetEvent(tick, _projectileId, _casterPlayerId, koProjectile.Position, velocity, koConfig.ProjectileSize);
        }

        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            DeactivateTalent(tick);
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            ref var projectile = ref _matchDataService.SimulationState.GetKOProjectileById(_projectileId);
            var koConfig = _gamePlayConfig.Talents.KOTalentConfig;

            if (_isInReturnPhase)
            {
                var distanceProjectileCenterToPlayerCenter = Vector2.DistanceSquared(projectile.Position, casterPlayerState.Spaceship.Transform.Position);
                var neededReachDistance = koConfig.ProjectileSize + casterPlayerState.Spaceship.Transform.Radius;
                var didReachPlayerCaster = distanceProjectileCenterToPlayerCenter <= neededReachDistance * neededReachDistance;

                if (didReachPlayerCaster)
                {
                    DeactivateTalent(tick);
                }
                else
                {
                    var directionToCaster = Vector2.Normalize(casterPlayerState.Spaceship.Transform.Position - projectile.Position);
                    projectile.Velocity = directionToCaster * koConfig.ProjectileSpeed * koConfig.ReturnSpeedMultiplier;
                    projectile.Rotation = directionToCaster * -1;
                }
            }
            else
            {
                var elapsedSeconds = (tick - projectile.CreatedOnTick) * deltaTime;
                if (elapsedSeconds >= koConfig.MaxFirstPhaseDuration)
                {
                    StartReturnPhase();
                }
            }
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            _projectileId = 0;
            _isInReturnPhase = false;
        }

        public void HitEnemyPlayer(ushort enemyPlayerId, int tick)
        {
            if (!IsCurrentlyActive || _isInReturnPhase)
            {
                return;
            }

            var koConfig = _gamePlayConfig.Talents.KOTalentConfig;
            ref var projectile = ref _matchDataService.SimulationState.GetKOProjectileById(_projectileId);
            var enemyPlayerState = _matchDataService.SimulationState.GetPlayerById(enemyPlayerId);
            var pushDirection = projectile.Velocity.Normalize();
            var pushForce = pushDirection * koConfig.PushForce;
            var randomSpin = RNG.NextFloat(koConfig.MinSpin, koConfig.MaxSpin);

            enemyPlayerState.Spaceship.Transform.Velocity += pushForce;
            enemyPlayerState.Spaceship.Transform.Direction = pushDirection;
            enemyPlayerState.Spaceship.IsEngineOn = false;

            _spinPlayerCommand
                .SetPlayer(enemyPlayerId)
                .SetSpinAmount(randomSpin)
                .SetTick(tick)
                .Execute();

            _netEventsDataService.AddKOProjectHitPlayerNetEvent(tick, _projectileId, enemyPlayerState.Id, projectile.Position);
            StartReturnPhase();
        }

        public void HitWall()
        {
            if (!IsCurrentlyActive || _isInReturnPhase)
            {
                return;
            }

            StartReturnPhase();
        }

        private void StartReturnPhase()
        {
            _isInReturnPhase = true;
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            _isInReturnPhase = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.KO, out int talentIndex))
            {
                LogService.LogError($"No KO talent found for player id {_casterPlayerId}");
                return;
            }
            ref var koTalentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);

            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, koTalentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            koTalentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _physicsSimulator.RemoveKOProjectile(_projectileId);
            _matchDataService.SimulationState.RemoveKOProjectileById(_projectileId);
            _netEventsDataService.AddDeactivateKOTalentNetEvent(tick, _casterPlayerId, _projectileId, cooldownEndTick);
        }
    }
}
