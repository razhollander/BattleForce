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
            for (int i = 0; i < _matchDataService.SimulationState.Bullets.Count; i++)
            {
                ref var bulletState = ref _matchDataService.SimulationState.GetBulletByIndex(i);
                bulletState.Position += bulletState.Velocity * _networkConfig.DeltaTime;
            }
        }
    }
}