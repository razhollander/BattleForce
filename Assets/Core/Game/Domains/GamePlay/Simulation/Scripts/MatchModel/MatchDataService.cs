using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private readonly SimulationStateS2C _simulationState;
        public SimulationStateS2C SimulationState => _simulationState;
        private ushort _lastBulletCreatedId = 0;
   
        public MatchDataService(NetworkConfig networkConfig)
        {
            _simulationState = new SimulationStateS2C(
                networkConfig.MaxCap.ConcurrentPlayers,
                networkConfig.MaxCap.ConcurrentBullets,
                networkConfig.MaxCap.ConcurrentEvironmentWalls,
                networkConfig.MaxCap.PointsInEvironmentWall);
        }

        public ref PlayerStateS2C AddPlayer(string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius, ushort health,
            float shootCooldown, Color color)
        {
            ref var newPlayer = ref _simulationState.Players.AddAndGet();
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
            return ref newPlayer;
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
    }
}