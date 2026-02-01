using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private readonly MatchSimulationStateS2C _simulationState;
        public MatchSimulationStateS2C SimulationState => _simulationState;
        private ushort _lastBulletCreatedId = 0;
        private ushort _lastPowerUpBallCreatedId = 0;
        private readonly MatchEnvironmentDataService _environmentDataService;
        public MatchEnvironmentDataService Environment => _environmentDataService;
        public bool IsMatchEnded { get; set; }

        public MatchDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, SimulationGamePlayConfig gamePlayConfig)
        {
            var chosenEnvironmentIndex = gamePlayConfig.ChosenEnvironmentIndex;
            _environmentDataService = new MatchEnvironmentDataService(sharedGamePlayConfig);
            _simulationState = new MatchSimulationStateS2C(
                networkConfig.MaxCap.ConcurrentPlayers,
                networkConfig.MaxCap.ConcurrentBullets,
                sharedGamePlayConfig.MaxConcurrentTalentsForPlayer,
                networkConfig.MaxCap.ConcurrentTalentCards,
                networkConfig.MaxCap.ConcurrentPowerUpBalls);

            _simulationState.EnvironmentLayoutIndex = chosenEnvironmentIndex;
        }

        public void InitEntryPoint()
        {
            _environmentDataService.InitEntryPoint(_simulationState.EnvironmentLayoutIndex);
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
            return newPlayer;
        }

        public PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius)
        {
            ref var playerBullet = ref _simulationState.Bullets.AddAndGet();
            var bulletId =(ushort) (++_lastBulletCreatedId % ushort.MaxValue);
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
    }
}