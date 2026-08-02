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
        private readonly TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;

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
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedConfig = sharedConfig;
            _tryAddForceToPlayerCommand = commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
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
                    if (currentProjectile.HitData.IsHookAttached)
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
                if (projectile.HitData.IsHookAttached)
                {
                    projectile = UpdateHookPositionRelativeToAttachedEntity(projectile);

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
                            var force = directionToHook * config.PlayerPullForceWhileHooked * deltaTime;
                            _tryAddForceToPlayerCommand.SetPlayerId(_casterPlayerId).SetForce(force).Execute();
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

        private TalentGrapplingHookProjectileStateS2C UpdateHookPositionRelativeToAttachedEntity(TalentGrapplingHookProjectileStateS2C projectile)
        {
            if (TryGetAttachedEntityTransform(projectile.HitData.HitType, projectile.HitData.AttachedEntityId, out var entityPosition, out var entityRotationRadians))
            {
                projectile.Position = LocalToWorld(projectile.HitData.AttachedLocalPosition, entityPosition, entityRotationRadians);
            }

            return projectile;
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            _projectileId = 0;
            _isInReturnPhase = false;
        }

        public void Hit(GrapplingHookHitType hitType, ushort attachedEntityId, int tick)
        {
            if (!IsCurrentlyActive || _isInReturnPhase)
            {
                return;
            }

            ref var projectile = ref _matchDataService.SimulationState.GetGrapplingHookProjectileById(_projectileId);
            if (projectile.HitData.IsHookAttached)
            {
                return;
            }

            _attachedOnTick = tick;
            projectile.HitData.IsHookAttached = true;
            projectile.HitData.HitType = hitType;
            projectile.HitData.AttachedEntityId = attachedEntityId;
            projectile.Velocity = Vector2.Zero;

            if (TryGetAttachedEntityTransform(hitType, attachedEntityId, out var entityPosition, out var entityRotationRadians))
            {
                projectile.HitData.AttachedLocalPosition = WorldToLocal(projectile.Position, entityPosition, entityRotationRadians);
            }

            _physicsSimulator.EnablePlayerToCollideWithPlayers(_casterPlayerId);
            _netEventsDataService.AddGrapplingHookHitWallNetEvent(tick, _projectileId, attachedEntityId, projectile.Position);
        }
        
        private bool TryGetAttachedEntityTransform(GrapplingHookHitType hitType, ushort attachedEntityId, out Vector2 position, out float rotationRadians)
        {
            switch (hitType)
            {
                case GrapplingHookHitType.Wall:
                    if (_matchDataService.EnvironmentData.TryGetEnvironmentWall(attachedEntityId, out var wall))
                    {
                        position = wall.Transform.WorldPosition;
                        rotationRadians = wall.Transform.WorldRotationDegrees * (System.MathF.PI / 180f);
                        return true;
                    }
                    break;
                case GrapplingHookHitType.FrigidBlock:
                    if (_matchDataService.SimulationState.TryGetFrigidBlockById(attachedEntityId, out var frigidBlock))
                    {
                        position = frigidBlock.Position;
                        rotationRadians = System.MathF.Atan2(frigidBlock.Rotation.Y, frigidBlock.Rotation.X);
                        return true;
                    }
                    break;
                case GrapplingHookHitType.RockPlayer:
                    // Keep following only while the player is still a rock; once the rock ends the hook stays where it was.
                    if (_matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(attachedEntityId, TalentType.Rock))
                    {
                        var rockTransform = _matchDataService.SimulationState.GetPlayerById(attachedEntityId).Spaceship.Transform;
                        position = rockTransform.Position;
                        rotationRadians = System.MathF.Atan2(rockTransform.Direction.Y, rockTransform.Direction.X);
                        return true;
                    }
                    break;
            }

            position = default;
            rotationRadians = default;
            return false;
        }

        private static Vector2 WorldToLocal(Vector2 worldPosition, Vector2 entityPosition, float entityRotationRadians)
        {
            Vector2 diff = worldPosition - entityPosition;
            float cos = System.MathF.Cos(-entityRotationRadians);
            float sin = System.MathF.Sin(-entityRotationRadians);

            return new Vector2(
                diff.X * cos - diff.Y * sin,
                diff.X * sin + diff.Y * cos);
        }

        private static Vector2 LocalToWorld(Vector2 localPosition, Vector2 entityPosition, float entityRotationRadians)
        {
            float cos = System.MathF.Cos(entityRotationRadians);
            float sin = System.MathF.Sin(entityRotationRadians);

            return new Vector2(
                localPosition.X * cos - localPosition.Y * sin,
                localPosition.X * sin + localPosition.Y * cos) + entityPosition;
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
            _physicsSimulator.DisablePlayerToCollideWithPlayers(_casterPlayerId);
            _matchDataService.SimulationState.RemoveGrapplingHookProjectileById(_projectileId);
            _netEventsDataService.AddDeactivateGrapplingHookTalentNetEvent(tick, _casterPlayerId, _projectileId, cooldownEndTick);
        }
    }
}
