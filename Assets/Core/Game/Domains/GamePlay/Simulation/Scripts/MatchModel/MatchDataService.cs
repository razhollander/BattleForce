using System.Linq;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private SimulationStateS2C _simulationState;
        public SimulationStateS2C SimulationState => _simulationState;

        public MatchDataService(NetworkConfig networkConfig)
        {
            _simulationState = new SimulationStateS2C();
            _simulationState.Players = new PlayerStateS2C[networkConfig.MaxConnectedPlayers];
            _simulationState.Bullets = new PlayerBulletS2C[networkConfig.MaxConcurrentBullets];
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
    }
}