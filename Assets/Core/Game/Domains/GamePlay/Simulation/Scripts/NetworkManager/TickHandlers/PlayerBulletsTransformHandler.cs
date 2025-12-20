using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers
{
    public class PlayerBulletsTransformHandler : IPlayerBulletsTransformHandler
    {
        private readonly IMatchDataService _matchDataService;
        private readonly NetworkConfig _networkConfig;

        public PlayerBulletsTransformHandler(IMatchDataService matchDataService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _networkConfig = networkConfig;
        }
        
        public void UpdateBulletsTransform()
        {
            foreach (var bulletIndex in _matchDataService.SimulationState.Bullets.UsedIndices())
            {
                ref PlayerBulletS2C bulletState = ref _matchDataService.SimulationState.Bullets[bulletIndex];
                bulletState.Position += bulletState.Velocity * _networkConfig.DeltaTime;
            }
        }
    }
}