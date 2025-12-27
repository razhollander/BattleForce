using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers
{
    public class PlayersTransformHandler : IPlayersTransformHandler
    {
        private readonly IMatchDataService _matchDataService;
        private readonly NetworkConfig _networkConfig;

        public PlayersTransformHandler(IMatchDataService matchDataService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _networkConfig = networkConfig;
        }
        
        public void UpdatePlayerTransform()
        {
            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                ref var playerModel = ref _matchDataService.SimulationState.GetPlayerByIndex(i);
                playerModel.Spaceship.Transform.Position += playerModel.Spaceship.Transform.Velocity * _networkConfig.DeltaTime;
            }
        }
    }
}