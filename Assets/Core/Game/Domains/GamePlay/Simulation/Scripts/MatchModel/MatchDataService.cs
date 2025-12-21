using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private SimulationStateS2C _simulationState;
        private SimulationStateS2C _previousSimulationState;
        public SimulationStateS2C SimulationState => _simulationState;
        public SimulationStateS2C PreviousSimulationState => _previousSimulationState;
        private ushort _lastBulletCreatedId = 0;
   
        public MatchDataService(NetworkConfig networkConfig)
        {
            _simulationState = new SimulationStateS2C();
            _simulationState.Players = new PlayerStateS2C[networkConfig.MaxConnectedPlayers];
            _simulationState.Bullets = new StructPool<PlayerBulletS2C>(networkConfig.MaxConcurrentBullets);
        }

        public void CopySimulationStateIntoPrevious()
        {
            _previousSimulationState = _simulationState;
        }

        public PlayerStateS2C AddPlayer(string playerName, PlayerTransformStateS2C playerTransformStateS2C, int health, float shootCooldown)
        {
            var playerSpaceship = new PlayerSpaceshipStateS2C(playerTransformStateS2C, shootCooldown, health);
            var playerId = (ushort)(_simulationState.PlayersCount + 1);
            var newPlayer = new PlayerStateS2C(playerId, playerName, playerSpaceship);
            _simulationState.Players[_simulationState.PlayersCount] = newPlayer;
            _simulationState.PlayersCount++;
            return newPlayer;
        }

        public PlayerStateS2C GetPlayer(int playerId)
        {
            return _simulationState.Players.First(x => x.Id == playerId);
        }

        public void SetPlayer(int playerId, PlayerStateS2C playerModel)
        {
            for (int i = 0; i < _simulationState.Players.Length; i++)
            {
                if (_simulationState.Players[i].Id == playerId)
                {
                    _simulationState.Players[i] = playerModel;
                    return;
                }
            }
        }

        public void SetBullet(ushort bulletModelId, PlayerBulletS2C bulletModel)
        {
            foreach (var usedIndex in _simulationState.Bullets.UsedIndices())
            {
                if (_simulationState.Bullets[usedIndex].Id == bulletModelId)
                {
                    _simulationState.Bullets[usedIndex] = bulletModel;
                    return;
                }
            }
        }

        public PlayerBulletS2C GetBullet(int bulletId)
        {
            foreach (var index in _simulationState.Bullets.UsedIndices())
            {
                var bullet = _simulationState.Bullets[index];
                if (bullet.Id == bulletId)
                {
                    return bullet;
                }
            }

            return default;
        }

        public void RemoveBullet(ushort bulletModelId)
        {
            foreach (var usedIndex in _simulationState.Bullets.UsedIndices())
            {
                if (_simulationState.Bullets[usedIndex].Id == bulletModelId)
                {
                    _simulationState.Bullets.Return(usedIndex);
                    return;
                }
            }
        }

        public PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius)
        {
            _simulationState.Bullets.Rent(out var index);
            ref PlayerBulletS2C playerBullet = ref _simulationState.Bullets[index];
            var bulletId =(ushort) (_lastBulletCreatedId++ % ushort.MaxValue);
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