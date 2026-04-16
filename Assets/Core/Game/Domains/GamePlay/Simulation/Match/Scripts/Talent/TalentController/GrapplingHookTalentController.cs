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
using Core.Scripts.Extensions;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
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
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedConfig;

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

        public GrapplingHookTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedConfig)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedConfig = sharedConfig;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive)
            {
                if (isTalentInputPressed)
                {
                    ref var currentProjectile = ref _matchDataService.SimulationState.GetGrapplingHookProjectileById(_projectileId);
                    if (currentProjectile.IsAttached)
                    {
                        DeactivateTalent(tick);
                    }
                }
                return;
            }

            if (!isTalentInputPressed)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            IsCurrentlyActive = true;
            _isInReturnPhase = false;

            var config = _gamePlayConfig.Talents.GrapplingHookTalentConfig;
            var direction = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var velocity = direction * config.ProjectileSpeed;
            var size = _sharedConfig.GrapplingHookProjectileSize;
            var projectilePosition = casterPlayerState.Spaceship.Transform.Position + direction * casterPlayerState.Spaceship.Transform.Radius;
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
            var config = _gamePlayConfig.Talents.GrapplingHookTalentConfig;
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
                    var directionToCaster = Vector2.Normalize(casterPlayerState.Spaceship.Transform.Position - projectile.Position);
                    projectile.Velocity = directionToCaster * config.ProjectileSpeed * config.ReturnProjectileSpeedMultiplier;
                }
            }
            else
            {
                if (projectile.IsAttached)
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
                            var directionToHook = Vector2.Normalize(projectile.Position - casterPlayerState.Spaceship.Transform.Position);
                            casterPlayerState.Spaceship.Transform.Velocity += directionToHook * config.PlayerPullForceWhileHooked * deltaTime;

                            casterPlayerState.Spaceship.Transform.Direction = MathUtils.RotateTowards(casterPlayerState.Spaceship.Transform.Direction,
                                casterPlayerState.Spaceship.Transform.Velocity, config.PlayerRotateSpeedWhileHooked * deltaTime);

                        }
                    }
                }
                else
                {
                    var distanceTraveled = Vector2.Distance(projectile.StartPosition, projectile.Position);
                    var didHookReachMaxDistance = distanceTraveled >= config.MaxDistance;

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
            if (projectile.IsAttached)
            {
                return;
            }

            _attachedOnTick = tick;
            projectile.IsAttached = true;
            projectile.Velocity = Vector2.Zero;
            projectile.AttachedWallId = wallId;

            // Calculate relative offset to wall
            if (_matchDataService.EnvironmentData.TryGetEnvironmentWall(wallId, out var wall))
            {
                Vector2 diff = projectile.Position - wall.Transform.WorldPosition;
                float radians = -wall.Transform.WorldRotationDegrees * (System.MathF.PI / 180f);
                float cos = System.MathF.Cos(radians);
                float sin = System.MathF.Sin(radians);

                projectile.AttachedLocalPosition = new Vector2(
                    diff.X * cos - diff.Y * sin,
                    diff.X * sin + diff.Y * cos
                );
            }
            
            //_physicsSimulator.UpdateGrapplingHookProjectile(_projectileId, projectile.Position, Vector2.Zero);
            _netEventsDataService.AddGrapplingHookHitWallNetEvent(tick, _projectileId, wallId, projectile.Position);
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
