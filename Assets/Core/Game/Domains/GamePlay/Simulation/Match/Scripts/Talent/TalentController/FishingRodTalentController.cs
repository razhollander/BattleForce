using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
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
        private readonly IPlayersMouseDataService _playersMouseDataService;
        private TrySpinPlayerCommand _trySpinPlayerCommand;
        private TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;
        private TryHitMoleCommand _tryHitMoleCommand;

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
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, SharedGamePlayConfig sharedConfig, ICommandFactory commandFactory, IPlayersMouseDataService playersMouseDataService)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedConfig = sharedConfig;
            _commandFactory = commandFactory;
            _playersMouseDataService = playersMouseDataService;
        }

        public void InitEntryPoint()
        {
            _trySpinPlayerCommand = _commandFactory.CreateCommandVoid<TrySpinPlayerCommand>();
            _tryAddForceToPlayerCommand = _commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
            _tryHitMoleCommand = _commandFactory.CreateCommandVoid<TryHitMoleCommand>();
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

            switch (projectile.Phase)
            {
                case FishingRodTipPhase.FlyingForward:
                    if (wasTalentInputDownThisTick)
                    {
                        StartReturnPhase(ref projectile);
                    }
                    break;
                case FishingRodTipPhase.CaughtEnemy:
                    ProcessThrowCastInput(wasTalentInputDownThisTick, tick, casterPlayerState, ref projectile);
                    break;
            }
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

        private void ProcessThrowCastInput(bool wasTalentInputDownThisTick, int tick, PlayerStateS2C casterPlayerState,
            ref TalentFishingRodProjectileStateS2C projectile)
        {
            if (!wasTalentInputDownThisTick)
            {
                return;
            }

            if (projectile.CaughtEnemyType == FishingRodCaughtEnemyType.Mole)
            {
                PerformSpinCaughtMole(tick, casterPlayerState, ref projectile);
                return;
            }

            PerformThrowEnemy(tick, casterPlayerState, ref projectile);
        }

        private void ActivateTalent(int tick, PlayerStateS2C casterPlayerState)
        {
            IsCurrentlyActive = true;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var aimDirection = casterPlayerState.Spaceship.AimDirection;
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
                case FishingRodTipPhase.FlyingForward:
                    UpdateFlyingPhase(tick, ref projectile);
                    break;
                case FishingRodTipPhase.CaughtEnemy:
                    UpdateCaughtPhase(tick, casterPlayerState, ref projectile);
                    break;
                case FishingRodTipPhase.ReturningBackwards:
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
            if (projectile.CaughtEnemyType == FishingRodCaughtEnemyType.Mole)
            {
                // A mole never moves, so the tip stays where it hooked it - only its disappearance has to be noticed here.
                if (!_matchDataService.SimulationState.TryGetMoleIndexById(projectile.CaughtEnemyId, out _)) // the hooked mole expired or was whacked by someone else
                {
                    DeactivateTalent(tick);
                    return;
                }
            }
            else
            {
                var caughtEnemy = _matchDataService.SimulationState.GetPlayerById(projectile.CaughtEnemyId);
                projectile.Position = caughtEnemy.Spaceship.Transform.Position;
            }

            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var throwWindowEndTick = TickUtils.GetTickPassedAfterDuration(_caughtOnTick, config.ThrowWindowSeconds, _networkConfig.DeltaTime);
            var didThrowWindowEnd = tick >= throwWindowEndTick;

            if (didThrowWindowEnd)
            {
                // The projectile (and its arrow) is removed by DeactivateTalent, so no explicit arrow clear is needed.
                DeactivateTalent(tick);
                return;
            }

            // The throw-aim arrow lives on the projectile so multiple rods catching the same enemy each show their own arrow.
            projectile.EnemyCaughtArrowDirection = IsCurrentlyAiming
                ? GetThrowDirection(casterPlayerState, projectile.Position)
                : Vector2.Zero;
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
            if (projectile.Phase != FishingRodTipPhase.FlyingForward)
            {
                return;
            }

            projectile.Phase = FishingRodTipPhase.CaughtEnemy;
            projectile.CaughtEnemyId = enemyPlayerId;
            projectile.CaughtEnemyType = FishingRodCaughtEnemyType.Player;
            projectile.Velocity = Vector2.Zero;
            projectile.Position = _matchDataService.SimulationState.GetPlayerById(enemyPlayerId).Spaceship.Transform.Position;
            _caughtOnTick = tick;

            // The caster starts aiming the throw immediately on catch, so a single talent press then throws the enemy.
            IsCurrentlyAiming = true;

            _physicsSimulator.RemoveFishingRodTip(_projectileId);
            _netEventsDataService.AddFishingRodCaughtEnemyNetEvent(tick, _projectileId, _casterPlayerId, enemyPlayerId, FishingRodCaughtEnemyType.Player);
        }

        public void CatchMole(ushort moleId, int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            ref var projectile = ref _matchDataService.SimulationState.GetFishingRodProjectileById(_projectileId);
            if (projectile.Phase != FishingRodTipPhase.FlyingForward)
            {
                return;
            }

            if (!_matchDataService.SimulationState.TryGetMoleById(moleId, out var mole))
            {
                return;
            }

            projectile.Phase = FishingRodTipPhase.CaughtEnemy;
            projectile.CaughtEnemyId = moleId;
            projectile.CaughtEnemyType = FishingRodCaughtEnemyType.Mole;
            projectile.Velocity = Vector2.Zero;
            projectile.Position = mole.Position;
            _caughtOnTick = tick;

            // The caster starts aiming the second cast immediately on catch, so a single talent press then spins the mole.
            IsCurrentlyAiming = true;

            _physicsSimulator.RemoveFishingRodTip(_projectileId);
            _netEventsDataService.AddFishingRodCaughtEnemyNetEvent(tick, _projectileId, _casterPlayerId, moleId, FishingRodCaughtEnemyType.Mole);
        }

        public void HitWall(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            ref var projectile = ref _matchDataService.SimulationState.GetFishingRodProjectileById(_projectileId);
            if (projectile.Phase != FishingRodTipPhase.FlyingForward)
            {
                return;
            }

            _netEventsDataService.AddFishingRodTipHitWallNetEvent(tick, _projectileId, projectile.Position);
            StartReturnPhase(ref projectile);
        }

        // A mole is nailed to its hole, so there is nowhere to throw it - the second cast simply spins it in place, and that is what damages it.
        private void PerformSpinCaughtMole(int tick, PlayerStateS2C casterPlayerState, ref TalentFishingRodProjectileStateS2C projectile)
        {
            _tryHitMoleCommand
                .SetMoleId(projectile.CaughtEnemyId)
                .SetByPlayerId(_casterPlayerId)
                .SetByTeamId(casterPlayerState.TeamId)
                .SetProcessedTick(tick)
                .Execute();

            _netEventsDataService.AddFishingRodThrowNetEvent(tick, _casterPlayerId, projectile.CaughtEnemyId, FishingRodCaughtEnemyType.Mole, GetThrowDirection(casterPlayerState, projectile.Position));
            DeactivateTalent(tick);
        }

        private void PerformThrowEnemy(int tick, PlayerStateS2C casterPlayerState, ref TalentFishingRodProjectileStateS2C projectile)
        {
            var config = _gamePlayConfigService.GamePlayConfig.Talents.FishingRodTalentConfig;
            var caughtEnemy = _matchDataService.SimulationState.GetPlayerById(projectile.CaughtEnemyId);
            var throwDirection = GetThrowDirection(casterPlayerState, caughtEnemy.Spaceship.Transform.Position);
            var force = throwDirection * config.ThrowPushForce;
            var spinAmount = RNG.NextFloat(config.ThrowMinSpin, config.ThrowMaxSpin);

            _trySpinPlayerCommand.SetPlayer(caughtEnemy.Id).SetSpinAmount(spinAmount).SetTick(tick).Execute();
            _tryAddForceToPlayerCommand.SetPlayerId(caughtEnemy.Id).SetForce(force).ShouldTurnOffEngine(true).Execute();
            
            _netEventsDataService.AddFishingRodThrowNetEvent(tick, _casterPlayerId, caughtEnemy.Id, FishingRodCaughtEnemyType.Player, throwDirection);
            DeactivateTalent(tick);
        }

        private Vector2 GetThrowDirection(PlayerStateS2C casterPlayerState, Vector2 caughtTargetPosition)
        {
            var mouseData = _playersMouseDataService.GetPlayerMouseData(_casterPlayerId);

            if (mouseData.IsUsingMouseAim)
            {
                return (mouseData.MouseWorldPosition - caughtTargetPosition).NormalizeSafe();
            }

            return casterPlayerState.Spaceship.AimDirection;
        }

        private void StartReturnPhase(ref TalentFishingRodProjectileStateS2C projectile)
        {
            projectile.Phase = FishingRodTipPhase.ReturningBackwards;
            projectile.Velocity = Vector2.Zero;
            _physicsSimulator.RemoveFishingRodTip(_projectileId);
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

            if (phase == FishingRodTipPhase.FlyingForward)
            {
                _physicsSimulator.RemoveFishingRodTip(_projectileId);
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
