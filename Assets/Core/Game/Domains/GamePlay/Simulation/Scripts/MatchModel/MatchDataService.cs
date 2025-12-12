using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Scripts.Network;
using ModestTree;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private SimulationStateS2C _simulationState;
        private SimulationStateS2C _previousSimulationState;
        public SimulationStateS2C SimulationState => _simulationState;
        public SimulationStateS2C PreviousSimulationState => _previousSimulationState;
        private ushort _lastBulletCreatedId = 0;
        private readonly MatchNetEventsDataService _matchNetEventsDataService;
        public MatchNetEventsDataService EventsData => _matchNetEventsDataService;
        public MatchDataService(NetworkConfig networkConfig)
        {
            _simulationState = new SimulationStateS2C();
            _simulationState.Players = new PlayerStateS2C[networkConfig.MaxConnectedPlayers];
            _simulationState.Bullets = new StructPool<PlayerBulletS2C>(networkConfig.MaxConcurrentBullets);
            _matchNetEventsDataService = new();
        }

        public void CopySimulationStateIntoPrevious()
        {
            _previousSimulationState = _simulationState;
        }
        
        public PlayerStateS2C AddPlayer(string playerName, PlayerTransformStateS2C playerTransformStateS2C, int health, float shootCooldown)
        {
            var playerSpaceship = new PlayerSpaceshipStateS2C(playerTransformStateS2C, shootCooldown, health);
            var playersCount = _simulationState.PlayersCount;
            var newPlayer = new PlayerStateS2C(playersCount, playerName, playerSpaceship);
            _simulationState.Players[playersCount] = newPlayer;
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

        public PlayerBulletS2C AddBullet(ushort playerId, Vector2 position, Vector2 direction, float moveSpeed)
        {
            _simulationState.Bullets.Rent(out var index);
            ref PlayerBulletS2C playerBullet = ref _simulationState.Bullets[index];
            var bulletId =(ushort) (_lastBulletCreatedId++ % ushort.MaxValue);
            playerBullet.Id = bulletId;
            playerBullet.BelongToPlayerId = playerId;
            playerBullet.Position = position;
            playerBullet.Direction = direction;
            playerBullet.MoveSpeed = moveSpeed;
            _matchNetEventsDataService.AddBulletSpawnNetEvent(bulletId, playerId, position);
            return playerBullet;
        }
    }
}