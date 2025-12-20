using Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers
{
    public class PlayersTransformHandler : IPlayersTransformHandler
    {
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;

        public PlayersTransformHandler(IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
        }
        
        public void UpdatePlayerTransform()
        {
            for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var player = _matchDataService.SimulationState.Players[i];
                var playerId = player.Id;
                var playerModel = _matchDataService.GetPlayer(playerId);
                playerModel.Spaceship.Transform.Position += playerModel.Spaceship.Transform.Direction *
                                                            _gamePlayConfig.PlayerSpaceship.MovementSpeed *
                                                            _networkConfig.DeltaTime;
                _matchDataService.SetPlayer(playerId, playerModel);
            }
        }
    }
}