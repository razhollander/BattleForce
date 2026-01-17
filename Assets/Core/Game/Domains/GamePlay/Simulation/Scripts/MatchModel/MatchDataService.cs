using Core.Game.Domains.GamePlay.Shared.MatchData.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private readonly SimulationStateS2C _simulationState;
        public SimulationStateS2C SimulationState => _simulationState;
        private ushort _lastBulletCreatedId = 0;
        public readonly FixedClassUnorderedList<MatchEnvironmentWallModel> Walls;

        public MatchDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, SimulationGamePlayConfig gamePlayConfig)
        {
            var chosenEnvironmentIndex = gamePlayConfig.ChosenWallsIndex;

            _simulationState = new SimulationStateS2C(
                networkConfig.MaxCap.ConcurrentPlayers,
                networkConfig.MaxCap.ConcurrentBullets,
                sharedGamePlayConfig.MaxConcurrentTalentsForPlayer,
                networkConfig.MaxCap.ConcurrentTalentCards);

            _simulationState.EnvironmentLayoutIndex = chosenEnvironmentIndex;
            
            ushort wallId = 1;
            Walls = new FixedClassUnorderedList<MatchEnvironmentWallModel>(networkConfig.MaxCap.ConcurrentEvironmentWalls,
                () => new MatchEnvironmentWallModel(wallId++, new Vector2[networkConfig.MaxCap.PointsInEvironmentWall]));
        }

        public PlayerStateS2C AddPlayer(string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius, ushort health,
            float shootCooldown, Color color)
        {
            var newPlayer = _simulationState.Players.AddAndGet();
            var playerId = (ushort)(_simulationState.Players.Count);
            newPlayer.Id = playerId;
            newPlayer.Name = playerName;
            newPlayer.TeamId = playerId;
            newPlayer.Spaceship.Health.CurrentHealth = health;
            newPlayer.Spaceship.Health.MaxHealth = health;
            newPlayer.Spaceship.Transform.Position = position;
            newPlayer.Spaceship.Transform.Direction = direction;
            newPlayer.Spaceship.Transform.Velocity = velocity;
            newPlayer.Spaceship.Transform.Radius = radius;
            newPlayer.Spaceship.Shoot.CooldownSecondsLeft = shootCooldown;
            newPlayer.Spaceship.Shoot.MaxCooldown = shootCooldown;
            newPlayer.Spaceship.Color = color;
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
        
        public void AddWall(ushort wallId, Vector2[] wallPoints)
        {
            var wallState = Walls.AddAndGet();
            wallState.Id = wallId;
            wallState.Points = wallPoints;
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
    }
}