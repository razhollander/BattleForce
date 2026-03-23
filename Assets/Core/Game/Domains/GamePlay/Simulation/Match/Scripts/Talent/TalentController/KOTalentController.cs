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

        public TalentType TalentType => TalentType.KO;
        public bool IsCurrentlyActive { get; private set; }

        // State tracking
        private bool _isReturning;
        private int _startTick;

        public KOTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive || !isTalentInputPressed)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            IsCurrentlyActive = true;
            _isReturning = false;
            _startTick = tick;

            var koConfig = _gamePlayConfig.Talents.KOTalentConfig;
            var direction = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var velocity = direction * koConfig.ProjectileSpeed;

            var koProjectile = _matchDataService.AddKOProjectile(tick, _casterPlayerId, casterPlayerState.Spaceship.Transform.Position, direction, velocity, koConfig.ProjectileSize);
            _projectileId = koProjectile.Id;

            _physicsSimulator.AddKOProjectile(_projectileId, koProjectile.Position, koConfig.ProjectileSize);
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

        public void OnTick(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            ref var projectile = ref _matchDataService.SimulationState.GetKOProjectileById(_projectileId);

            var koConfig = _gamePlayConfig.Talents.KOTalentConfig;

            if (!_isReturning)
            {
                var elapsedSeconds = (tick - _startTick) * _networkConfig.DeltaTime;
                if (elapsedSeconds >= koConfig.MaxDuration)
                {
                    StartReturnPhase(tick);
                }
                else
                {
                    projectile.Position += projectile.Velocity * _networkConfig.DeltaTime;
                    _physicsSimulator.UpdateKOProjectile(_projectileId, projectile.Position);
                }
            }
            else
            {
                var directionToCaster = Vector2.Normalize(casterPlayerState.Spaceship.Transform.Position - projectile.Position);
                projectile.Velocity = directionToCaster * koConfig.ProjectileSpeed * koConfig.ReturnSpeedMultiplier;
                projectile.Position += projectile.Velocity * _networkConfig.DeltaTime;

                var distanceSqr = Vector2.DistanceSquared(projectile.Position, casterPlayerState.Spaceship.Transform.Position);
                if (distanceSqr <= (koConfig.ProjectileSize + casterPlayerState.Spaceship.Transform.Radius) * (koConfig.ProjectileSize + casterPlayerState.Spaceship.Transform.Radius))
                {
                    DeactivateTalent(tick);
                }
            }
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            _projectileId = 0;
            _isReturning = false;
        }

        public void PerformTalentWithEnemy(PlayerStateS2C enemyPlayer, int tick)
        {
            if (!IsCurrentlyActive || _isReturning)
            {
                return;
            }

            var koConfig = _gamePlayConfig.Talents.KOTalentConfig;
            ref var projectile = ref _matchDataService.SimulationState.GetKOProjectileById(_projectileId);

            var pushDirection = Vector2.Normalize(enemyPlayer.Spaceship.Transform.Position - projectile.Position);
            enemyPlayer.Spaceship.Transform.Velocity += pushDirection * koConfig.PushForce;
            enemyPlayer.Spaceship.Transform.AngularVelocity += koConfig.SpinForce;
            enemyPlayer.Spaceship.EngineOffSecondsLeft = koConfig.DurationEngineOffSeconds;

            _netEventsDataService.AddKOProjectHitPlayerNetEvent(tick, _projectileId, enemyPlayer.Id, projectile.Position);

            StartReturnPhase(tick);
        }

        public void HitWall(int tick)
        {
            if (!IsCurrentlyActive || _isReturning)
            {
                return;
            }

            StartReturnPhase(tick);
        }

        private void StartReturnPhase(int tick)
        {
            if (!_isReturning)
            {
                _isReturning = true;
                _physicsSimulator.RemoveKOProjectile(_projectileId); // Disable collider
            }
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.KO, out int talentIndex))
            {
                LogService.LogError($"No KO talent found for player id {_casterPlayerId}");
                return;
            }
            ref var koTalentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);

            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, koTalentModel.MaxCooldown, _networkConfig.DeltaTime);
            koTalentModel.CooldownEndTick = cooldownEndTick;

            if (!_isReturning) // If not returning, we haven't removed it from physics yet
            {
                _physicsSimulator.RemoveKOProjectile(_projectileId);
            }

            _matchDataService.SimulationState.RemoveKOProjectileById(_projectileId);
            _netEventsDataService.AddDeactivateKOTalentNetEvent(tick, _casterPlayerId, _projectileId, cooldownEndTick);
        }
    }
}
