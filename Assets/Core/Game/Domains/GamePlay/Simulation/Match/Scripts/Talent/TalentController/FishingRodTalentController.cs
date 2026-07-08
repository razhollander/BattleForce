using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class FishingRodTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private ushort _projectileId;
        private int _tipFiredTick;
        private int _caughtOnTick;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedConfig;
        private readonly ICommandFactory _commandFactory;
        private SpinPlayerCommand _spinPlayerCommand;
        private AddForceToPlayerCommand _addForceToPlayerCommand;

        public TalentType TalentType => TalentType.FishingRod;

        private bool IsCurrentlyActive
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        private bool IsCurrentlyAiming
        {
            get => _matchDataService.SimulationState.GetIsTalentAimingForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyAimingForPlayer(_casterPlayerId, TalentType, value);
        }

        public FishingRodTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedConfig = sharedConfig;
            _commandFactory = commandFactory;
        }

        public void InitEntryPoint()
        {
            _spinPlayerCommand = _commandFactory.CreateCommandVoid<SpinPlayerCommand>();
            _addForceToPlayerCommand = _commandFactory.CreateCommandVoid<AddForceToPlayerCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            var isCurrentlyAiming = IsCurrentlyAiming;

            if (!IsCurrentlyActive)
            {
                ProcessInitialCastInput(wasTalentInputDownThisTick, wasTalentInputReleasedThisTick, isCurrentlyAiming, tick, casterPlayerState);
                return;
            }

            ref var projectile = ref _matchDataService.SimulationState.GetFishingRodProjectileById(_projectileId);
            if (projectile.Phase != FishingRodTipPhase.CaughtEnemy)
            {
                return;
            }

            ProcessThrowCastInput(wasTalentInputDownThisTick, wasTalentInputReleasedThisTick, isCurrentlyAiming, tick, casterPlayerState, ref projectile);
        }

        private void ProcessInitialCastInput(bool wasTalentInputDownThisTick, bool wasTalentInputReleasedThisTick, bool isCurrentlyAiming, int tick, PlayerStateS2C casterPlayerState)
        {
            if (wasTalentInputDownThisTick && !isCurrentlyAiming)
            {
                IsCurrentlyAiming = true;
                casterPlayerState.Spaceship.AssistArrowType = PlayerAssistArrowType.AimArrow;
            }

            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }

            casterPlayerState.Spaceship.AssistArrowType = PlayerAssistArrowType.Hidden;
            IsCurrentlyAiming = false;
            ActivateTalent(tick, casterPlayerState);
        }

        private void ProcessThrowCastInput(bool wasTalentInputDownThisTick, bool wasTalentInputReleasedThisTick, bool isCurrentlyAiming, int tick, PlayerStateS2C casterPlayerState,
            ref TalentFishingRodProjectileStateS2C projectile)
        {
            if (wasTalentInputDownThisTick && !isCurrentlyAiming)
            {
                IsCurrentlyAiming = true;
            }

            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }

            IsCurrentlyAiming = false;
            PerformThrow(tick, casterPlayerState, ref projectile);
        }

        private void ActivateTalent(int tick, PlayerStateS2C casterPlayerState)
        {
            IsCurrentlyActive = true;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var aimDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var velocity = aimDirection * config.TipSpeed;
            var size = _sharedConfig.FishingRodTipSize;
            var tipPosition = casterPlayerState.Spaceship.Transform.Position + aimDirection * casterPlayerState.Spaceship.Transform.Radius;

            var projectile = _matchDataService.AddFishingRodProjectile(_casterPlayerId, tipPosition, velocity);
            _projectileId = projectile.Id;
            _tipFiredTick = tick;
            _physicsSimulator.AddFishingRodTip(_projectileId, casterPlayerState.TeamId, projectile.Position, size, velocity);
            _netEventsDataService.AddCreateFishingRodProjectileNetEvent(tick, _projectileId, _casterPlayerId, projectile.Position);
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            ref var projectile = ref _matchDataService.SimulationState.GetFishingRodProjectileById(_projectileId);

            switch (projectile.Phase)
            {
                case FishingRodTipPhase.Flying:
                    UpdateFlyingPhase(tick, ref projectile);
                    break;
                case FishingRodTipPhase.CaughtEnemy:
                    UpdateCaughtPhase(tick, casterPlayerState, ref projectile);
                    break;
                case FishingRodTipPhase.Returning:
                    UpdateReturnPhase(tick, deltaTime, casterPlayerState, ref projectile);
                    break;
            }
        }

        private void UpdateFlyingPhase(int tick, ref TalentFishingRodProjectileStateS2C projectile)
        {
            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var lifetimeEndTick = TickUtils.GetTickPassedAfterDuration(_tipFiredTick, config.TipMaxLifetimeSeconds, _networkConfig.DeltaTime);
            var didExceedLifetime = tick >= lifetimeEndTick;

            if (didExceedLifetime)
            {
                StartReturnPhase(ref projectile);
            }
        }

        private void UpdateCaughtPhase(int tick, PlayerStateS2C casterPlayerState, ref TalentFishingRodProjectileStateS2C projectile)
        {
            var caughtEnemy = _matchDataService.SimulationState.GetPlayerById(projectile.CaughtEnemyId);
            projectile.Position = caughtEnemy.Spaceship.Transform.Position;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var throwWindowEndTick = TickUtils.GetTickPassedAfterDuration(_caughtOnTick, config.ThrowWindowSeconds, _networkConfig.DeltaTime);
            var didThrowWindowEnd = tick >= throwWindowEndTick;

            if (didThrowWindowEnd)
            {
                ClearCaughtEnemyAim(caughtEnemy);
                DeactivateTalent(tick);
                return;
            }

            if (IsCurrentlyAiming)
            {
                caughtEnemy.Spaceship.AssistArrowType = PlayerAssistArrowType.SecondCastAimArrow;
                caughtEnemy.Spaceship.SecondCastAimDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
            }
            else
            {
                ClearCaughtEnemyAim(caughtEnemy);
            }
        }

        private void UpdateReturnPhase(int tick, float deltaTime, PlayerStateS2C casterPlayerState, ref TalentFishingRodProjectileStateS2C projectile)
        {
            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var casterPosition = casterPlayerState.Spaceship.Transform.Position;
            var distanceToCasterSquared = Vector2.DistanceSquared(projectile.Position, casterPosition);
            var didReachCaster = distanceToCasterSquared <= config.ArriveDistance * config.ArriveDistance;

            if (didReachCaster)
            {
                DeactivateTalent(tick);
                return;
            }

            var directionToCaster = (casterPosition - projectile.Position).NormalizeSafe();
            projectile.Velocity = directionToCaster * config.TipSpeed * config.ReturnSpeedMultiplier;
            projectile.Position += projectile.Velocity * deltaTime;
        }

        public void CatchEnemy(ushort enemyPlayerId, int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            ref var projectile = ref _matchDataService.SimulationState.GetFishingRodProjectileById(_projectileId);
            if (projectile.Phase != FishingRodTipPhase.Flying)
            {
                return;
            }

            projectile.Phase = FishingRodTipPhase.CaughtEnemy;
            projectile.CaughtEnemyId = enemyPlayerId;
            projectile.Velocity = Vector2.Zero;
            projectile.Position = _matchDataService.SimulationState.GetPlayerById(enemyPlayerId).Spaceship.Transform.Position;
            _caughtOnTick = tick;

            _physicsSimulator.RemoveFishingRodTip(_projectileId);
            _netEventsDataService.AddFishingRodCaughtEnemyNetEvent(tick, _projectileId, _casterPlayerId, enemyPlayerId);
        }

        public void HitWall(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            ref var projectile = ref _matchDataService.SimulationState.GetFishingRodProjectileById(_projectileId);
            if (projectile.Phase != FishingRodTipPhase.Flying)
            {
                return;
            }

            _netEventsDataService.AddFishingRodTipHitWallNetEvent(tick, _projectileId, projectile.Position);
            StartReturnPhase(ref projectile);
        }

        private void PerformThrow(int tick, PlayerStateS2C casterPlayerState, ref TalentFishingRodProjectileStateS2C projectile)
        {
            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var caughtEnemy = _matchDataService.SimulationState.GetPlayerById(projectile.CaughtEnemyId);
            var throwDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var force = throwDirection * config.ThrowPushForce;
            var spinAmount = RNG.NextFloat(config.ThrowMinSpin, config.ThrowMaxSpin);

            _spinPlayerCommand.SetPlayer(caughtEnemy.Id).SetSpinAmount(spinAmount).SetTick(tick).Execute();
            _addForceToPlayerCommand.SetPlayerId(caughtEnemy.Id).SetForce(force).ShouldTurnOffEngine(true).Execute();

            ClearCaughtEnemyAim(caughtEnemy);
            _netEventsDataService.AddFishingRodThrowNetEvent(tick, _casterPlayerId, caughtEnemy.Id, throwDirection);
            DeactivateTalent(tick);
        }

        private void StartReturnPhase(ref TalentFishingRodProjectileStateS2C projectile)
        {
            projectile.Phase = FishingRodTipPhase.Returning;
            projectile.Velocity = Vector2.Zero;
            _physicsSimulator.RemoveFishingRodTip(_projectileId);
        }

        private void ClearCaughtEnemyAim(PlayerStateS2C caughtEnemy)
        {
            if (caughtEnemy.Spaceship.AssistArrowType == PlayerAssistArrowType.SecondCastAimArrow)
            {
                caughtEnemy.Spaceship.AssistArrowType = PlayerAssistArrowType.Hidden;
            }

            caughtEnemy.Spaceship.SecondCastAimDirection = Vector2.Zero;
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
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            ref var projectile = ref _matchDataService.SimulationState.GetFishingRodProjectileById(_projectileId);
            var phase = projectile.Phase;
            var caughtEnemyId = projectile.CaughtEnemyId;

            IsCurrentlyActive = false;
            IsCurrentlyAiming = false;

            int cooldownEndTick = tick;
            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.FishingRod, out int talentIndex))
            {
                LogService.LogError($"No FishingRod talent found for player id {_casterPlayerId}");
            }
            else
            {
                ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
                cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
                talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;
            }

            if (phase == FishingRodTipPhase.Flying)
            {
                _physicsSimulator.RemoveFishingRodTip(_projectileId);
            }
            else if (phase == FishingRodTipPhase.CaughtEnemy)
            {
                ClearCaughtEnemyAim(_matchDataService.SimulationState.GetPlayerById(caughtEnemyId));
            }

            _matchDataService.SimulationState.RemoveFishingRodProjectileById(_projectileId);
            _netEventsDataService.AddDeactivateFishingRodTalentNetEvent(tick, _casterPlayerId, _projectileId, cooldownEndTick);
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            IsCurrentlyAiming = false;
            _projectileId = 0;
        }
    }
}
