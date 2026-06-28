using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
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
using Core.Scripts.Extensions;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class GrapplingHookTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private ushort _projectileId;
        private int _attachedOnTick;
        private bool _isInReturnPhase;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedConfig;
        private readonly ICommandFactory _commandFactory;
        private readonly SpinPlayerCommand _spinPlayerCommand;

        public TalentType TalentType => TalentType.GrapplingHook;
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

        public GrapplingHookTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedConfig)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedConfig = sharedConfig;
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
                if (wasTalentInputDownThisTick)
                {
                    ref var currentProjectile = ref _matchDataService.SimulationState.GetGrapplingHookProjectileById(_projectileId);
                    if (currentProjectile.IsHookAttached)
                    {
                        DeactivateTalent(tick);
                    }
                }
                return;
            }

            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }

            casterPlayerState.Spaceship.AssistArrowType = Core.Game.Domains.GamePlay.Shared.Scripts.Enums.PlayerAssistArrowType.Hidden;
            IsCurrentlyAiming = false;
            ActivateTalent(tick, casterPlayerState);
        }

        private void ActivateTalent(int tick, PlayerStateS2C casterPlayerState)
        {
            IsCurrentlyActive = true;
            _isInReturnPhase = false;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.GrapplingHookTalentConfig;
            var aimDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var velocity = aimDirection * config.ProjectileSpeed;
            var size = _sharedConfig.GrapplingHookProjectileSize;
            var projectilePosition = casterPlayerState.Spaceship.Transform.Position + aimDirection * casterPlayerState.Spaceship.Transform.Radius;
            var projectile = _matchDataService.AddGrapplingHookProjectile(_casterPlayerId, projectilePosition, velocity);
            _projectileId = projectile.Id;
            _physicsSimulator.AddGrapplingHookProjectile(_projectileId, casterPlayerState.TeamId, projectile.Position, size, velocity);
            _netEventsDataService.AddCreateGrapplingHookProjectileNetEvent(tick, _projectileId, _casterPlayerId, projectile.Position);
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
            ref var projectile = ref _matchDataService.SimulationState.GetGrapplingHookProjectileById(_projectileId);
            var config = _gamePlayConfigService.GamePlayConfig.Talents.GrapplingHookTalentConfig;
            var arriveDistance = config.ArriveDistance;

            if (_isInReturnPhase)
            {
                var distanceProjectileCenterToPlayerCenter = Vector2.DistanceSquared(projectile.Position, casterPlayerState.Spaceship.Transform.Position);
                var neededReachDistance = arriveDistance;
                var didReachPlayerCaster = distanceProjectileCenterToPlayerCenter <= neededReachDistance * neededReachDistance;

                if (didReachPlayerCaster)
                {
                    DeactivateTalent(tick);
                }
                else
                {
                    var directionToCaster = (casterPlayerState.Spaceship.Transform.Position - projectile.Position).NormalizeSafe();
                    projectile.Velocity = directionToCaster * config.ProjectileSpeed * config.ReturnProjectileSpeedMultiplier;
                }
            }
            else
            {
                if (projectile.IsHookAttached)
                {
                    projectile = UpdateHookPositionRelativeToAttachedWall(projectile);

                    var playerToHookDistanceSquared = Vector2.DistanceSquared(casterPlayerState.Spaceship.Transform.Position, projectile.Position);
                    var didPlayerReachHook = playerToHookDistanceSquared <= arriveDistance * arriveDistance;

                    if (didPlayerReachHook)
                    {
                        var playerCaster = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
                        playerCaster.Spaceship.Transform.Velocity = Vector2.Zero;
                        DeactivateTalent(tick);
                    }
                    else
                    {
                        var didPassGraceTicks = tick > _attachedOnTick + config.GraceTicksUntilCheckIfVelocityIsBelowThreshold;
                        var isPlayerVelocityZero = casterPlayerState.Spaceship.Transform.Velocity.LengthSquared() < config.PlayerVelocitySquaredThresholdToDeactivateHook;

                        if (didPassGraceTicks && isPlayerVelocityZero)
                        {
                            DeactivateTalent(tick);
                        }
                        else
                        {
                            var directionToHook = (projectile.Position - casterPlayerState.Spaceship.Transform.Position).NormalizeSafe();
                            casterPlayerState.Spaceship.Transform.Velocity += directionToHook * config.PlayerPullForceWhileHooked * deltaTime;

                            // casterPlayerState.Spaceship.Transform.Direction = MathUtils.RotateTowards(casterPlayerState.Spaceship.Transform.Direction,
                            //     casterPlayerState.Spaceship.Transform.Velocity, config.PlayerRotateSpeedWhileHooked * deltaTime);

                        }
                    }
                }
                else
                {
                    var distanceTraveled = Vector2.Distance(projectile.StartPosition, projectile.Position);
                    var didHookReachMaxDistance = distanceTraveled >= _sharedConfig.GrapplingHookProjectileMaxDistance;

                    if (didHookReachMaxDistance)
                    {
                        StartReturnPhase();
                    }
                }
            }
        }

        private TalentGrapplingHookProjectileStateS2C UpdateHookPositionRelativeToAttachedWall(TalentGrapplingHookProjectileStateS2C projectile)
        {
            if (_matchDataService.EnvironmentData.TryGetEnvironmentWall(projectile.AttachedWallId, out var wall))
            {
                var radians = wall.Transform.WorldRotationDegrees * (System.MathF.PI / 180f);
                var cos = System.MathF.Cos(radians);
                var sin = System.MathF.Sin(radians);

                Vector2 localPos = projectile.AttachedLocalPosition;
                Vector2 worldPos = new Vector2(
                    localPos.X * cos - localPos.Y * sin,
                    localPos.X * sin + localPos.Y * cos
                ) + wall.Transform.WorldPosition;

                projectile.Position = worldPos;
            }

            return projectile;
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            _projectileId = 0;
            _isInReturnPhase = false;
        }

        public void HitWall(ushort wallId, int tick)
        {
            if (!IsCurrentlyActive || _isInReturnPhase)
            {
                return;
            }

            ref var projectile = ref _matchDataService.SimulationState.GetGrapplingHookProjectileById(_projectileId);
            if (projectile.IsHookAttached)
            {
                return;
            }

            _attachedOnTick = tick;
            projectile.IsHookAttached = true;
            projectile.Velocity = Vector2.Zero;

            if (TryGetLocalPositionForWall(wallId, projectile.Position, out var localPositionOfWall))
            {
                projectile.AttachedWallId = wallId;
                projectile.AttachedLocalPosition = localPositionOfWall;
            }
            
            _netEventsDataService.AddGrapplingHookHitWallNetEvent(tick, _projectileId, wallId, projectile.Position);
        }

        private bool TryGetLocalPositionForWall(ushort wallId, Vector2 worldPosition, out Vector2 localPositionOfWall)
        {
            if (_matchDataService.EnvironmentData.TryGetEnvironmentWall(wallId, out var wall))
            {
                Vector2 diff = worldPosition - wall.Transform.WorldPosition;
                float radians = -wall.Transform.WorldRotationDegrees * (System.MathF.PI / 180f);
                float cos = System.MathF.Cos(radians);
                float sin = System.MathF.Sin(radians);

                localPositionOfWall = new Vector2(
                    diff.X * cos - diff.Y * sin,
                    diff.X * sin + diff.Y * cos);

                return true;
            }

            localPositionOfWall = default;
            return false;
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

            int cooldownEndTick = tick;

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.GrapplingHook, out int talentIndex))
            {
                LogService.LogError($"No GrapplingHook talent found for player id {_casterPlayerId}");
            }
            else
            {
                ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
                cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
                talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;
            }

            _physicsSimulator.RemoveGrapplingHookProjectile(_projectileId);
            _matchDataService.SimulationState.RemoveGrapplingHookProjectileById(_projectileId);
            _netEventsDataService.AddDeactivateGrapplingHookTalentNetEvent(tick, _casterPlayerId, _projectileId, cooldownEndTick);
        }
    }
}
