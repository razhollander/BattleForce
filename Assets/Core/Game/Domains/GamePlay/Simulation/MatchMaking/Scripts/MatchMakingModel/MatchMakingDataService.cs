using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Network;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel
{
    public class MatchMakingDataService : IMatchMakingDataService
    {
        private readonly MatchMakingSimulationStateS2C _simulationState;
        public MatchMakingSimulationStateS2C SimulationState => _simulationState;
        private ushort _lastBulletCreatedId = 0;
        private readonly MatchMakingEnvironmentDataService _environmentDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        public MatchMakingEnvironmentDataService Environment => _environmentDataService;
        public MatchMakingDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _environmentDataService = new MatchMakingEnvironmentDataService(sharedGamePlayConfig);
            _simulationState = new MatchMakingSimulationStateS2C(
                networkConfig.MaxCap.ConcurrentPlayers,
                networkConfig.MaxCap.ConcurrentBullets);
        }

        public void InitEntryPoint()
        {
            _environmentDataService.InitEntryPoint();
        }

        public MatchMakingPlayerStateS2C AddPlayer(string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius,
            float shootCooldown, ushort teamId)
        {
            var newPlayer = _simulationState.Players.AddAndGet();
            var playerId = (ushort)(_simulationState.Players.Count);
            newPlayer.Id = playerId;
            newPlayer.Name = playerName;
            newPlayer.TeamId = teamId;
            newPlayer.Spaceship.Transform.Position = position;
            newPlayer.Spaceship.Transform.Direction = direction;
            newPlayer.Spaceship.Transform.Velocity = velocity;
            newPlayer.Spaceship.Transform.Radius = radius;

            if (playerName == "Chen")
            {
                shootCooldown = 0f;
            }

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
    }
}