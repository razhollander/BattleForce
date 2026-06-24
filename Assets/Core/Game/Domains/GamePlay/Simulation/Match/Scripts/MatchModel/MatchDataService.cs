using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private readonly MatchSimulationStateS2C _simulationState;
        public MatchSimulationStateS2C SimulationState => _simulationState;
        private ushort _lastBulletCreatedId = 0;
        private ushort _lastPowerUpBallCreatedId = 0;
        private ushort _lastSwapFieldCreatedId = 0;
        private ushort _lastKOProjectileCreatedId = 0;
        private ushort _lastGrapplingHookProjectileCreatedId = 0;
        private ushort _lastChickenEggCreatedId = 0;
        private ushort _lastGalacticForceFieldCreatedId = 0;
        public List<int> DidntPlayYetStageIndexes { get; } = new List<int>();
        public MatchEnvironmentDataService EnvironmentData { get; private set; }
        public HashSet<ushort> TeamIds { get; private set; }

        public MatchDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            EnvironmentData = new MatchEnvironmentDataService(networkConfig);
            var maxCap = networkConfig.MaxCap;

            _simulationState = new MatchSimulationStateS2C(
                maxCap.ConcurrentPlayers,
                maxCap.ConcurrentBullets,
                sharedGamePlayConfig.MaxConcurrentTalentsForPlayer,
                maxCap.ConcurrentTalentCards,
                maxCap.ConcurrentPowerUpBalls,
                sharedGamePlayConfig.MaxTeamsAmount,
                maxCap.ConcurrentChickenEggs,
                maxCap.ConcurrentGalacticForceFields);

            TeamIds = new HashSet<ushort>(sharedGamePlayConfig.MaxTeamsAmount);
            _simulationState.GemsPerTeamId = new Dictionary<ushort, int>(sharedGamePlayConfig.MaxTeamsAmount);
            _simulationState.StageType = StageType.DeathMatch;
        }
        
        public PlayerStateS2C AddPlayer(ushort playerId, ushort teamId, string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius, ushort health,
            float shootCooldown)
        {
            var newPlayer = _simulationState.Players.AddAndGet();
            newPlayer.Id = playerId;
            newPlayer.Name = playerName;
            newPlayer.TeamId = teamId;
            newPlayer.Spaceship.Health.CurrentHealth = health;
            newPlayer.Spaceship.Health.MaxHealth = health;
            newPlayer.Spaceship.Transform.Position = position;
            newPlayer.Spaceship.Transform.Direction = direction;
            newPlayer.Spaceship.Transform.Velocity = velocity;
            newPlayer.Spaceship.Transform.Radius = radius;
            newPlayer.Spaceship.Shoot.CooldownSecondsLeft = shootCooldown;
            newPlayer.Spaceship.Shoot.MaxCooldown = shootCooldown;
            TeamIds.Add(teamId);
            _simulationState.GemsPerTeamId.TryAdd(teamId, 0);
            _simulationState.BoltsPerTeam.TryAdd(teamId, 0);
            return newPlayer;
        }

        public PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius, int createdOnTick)
        {
            ref var playerBullet = ref _simulationState.Bullets.AddAndGet();
            var bulletId =(ushort) (++_lastBulletCreatedId % ushort.MaxValue);
            playerBullet.CreatedOnTick = createdOnTick;
            playerBullet.Id = bulletId;
            playerBullet.BelongToPlayerId = belongToPlayerId;
            playerBullet.Position = position;
            playerBullet.Direction = direction;
            playerBullet.Radius = radius;
            playerBullet.Velocity = direction * moveSpeed;
            return playerBullet;
        }

        public TalentCardS2C AddTalentCard(ushort talentCardId, Vector2 position, TalentType talentType, ushort Health)
        {
            ref var newCard = ref _simulationState.TalentCards.AddAndGet();
            newCard.Id = talentCardId;
            newCard.Position = position;
            newCard.TalentType = talentType;
            newCard.Health = Health;
            return newCard;
        }

        public PowerUpBallS2C AddPowerUpBall(Vector2 position, Vector2 velocity, PowerUpType powerUpType)
        {
            ref var powerUpBall = ref _simulationState.PowerUpBalls.AddAndGet();
            var powerUpBallId =(ushort) (++_lastPowerUpBallCreatedId % ushort.MaxValue);
            powerUpBall.Id = powerUpBallId;
            powerUpBall.Position = position;
            powerUpBall.Velocity = velocity;
            return powerUpBall;
        }

        public TalentSwapFieldS2C AddSwapField(ushort casterPlayerId, int tick, int fieldEndTick)
        {
            ref var swapField = ref _simulationState.SwapFields.AddAndGet();
            var swapFieldId =(ushort) (++_lastSwapFieldCreatedId % ushort.MaxValue);
            swapField.Id = swapFieldId;
            swapField.PlayerCasterId = casterPlayerId;
            swapField.CreatedOnTick = tick;
            swapField.EndTick = fieldEndTick;
            return swapField;
        }

        public TalentKOProjectileS2C AddKOProjectile(int tick, ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 velocity, float size)
        {
            ref var koProjectile = ref _simulationState.KOProjectiles.AddAndGet();
            var koProjectileId = (ushort)(++_lastKOProjectileCreatedId % ushort.MaxValue);
            koProjectile.CreatedOnTick = tick;
            koProjectile.Id = koProjectileId;
            koProjectile.PlayerCasterId = casterPlayerId;
            koProjectile.Position = position;
            koProjectile.Rotation = rotation;
            koProjectile.Velocity = velocity;
            koProjectile.Size = size;
            return koProjectile;
        }

        public TalentGrapplingHookProjectileStateS2C AddGrapplingHookProjectile(ushort casterPlayerId, Vector2 position, Vector2 velocity)
        {
            ref var grapplingHookProjectile = ref _simulationState.GrapplingHookProjectiles.AddAndGet();
            var projectileId = (ushort)(++_lastGrapplingHookProjectileCreatedId % ushort.MaxValue); 
            grapplingHookProjectile.Id = projectileId;
            grapplingHookProjectile.PlayerCasterId = casterPlayerId;
            grapplingHookProjectile.StartPosition = position;
            grapplingHookProjectile.Position = position;
            grapplingHookProjectile.Velocity = velocity;
            grapplingHookProjectile.IsHookAttached = false;
            return grapplingHookProjectile;
        }

        public TalentChickenEggStateS2C AddChickenEgg(ushort casterPlayerId, Vector2 position)
        {
            ref var egg = ref SimulationState.ChickenEggs.AddAndGet();
            var eggId = (ushort)(++_lastChickenEggCreatedId % ushort.MaxValue);
            egg.Id = eggId;
            egg.PlayerCasterId = casterPlayerId;
            egg.Position = position;

            return egg;
        }

        public GalacticForceFieldS2C AddGalacticForceField(ushort casterPlayerId, ushort casterTeamId, int endTick)
        {
            ref var field = ref SimulationState.GalacticForceFields.AddAndGet();
            var fieldId = (ushort)(++_lastGalacticForceFieldCreatedId % ushort.MaxValue);
            field.Id = fieldId;
            field.CasterPlayerId = casterPlayerId;
            field.CasterTeamId = casterTeamId;
            field.EndTick = endTick;
            return field;
        }
    }
}