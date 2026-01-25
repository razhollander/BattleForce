using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel
{
    public class MatchMakingEnvironmentDataService
    {
        public Vector2 EnvironmentHalfSize { get; private set; }
        public WallConfig[] WallConfigs { get; private set; }
        
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        
        public MatchMakingEnvironmentDataService(SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void InitEntryPoint(int environmentIndex)
        {
            WallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentIndex).GetWalls();
            EnvironmentHalfSize = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentIndex).GetEnvironmentHalfSize();
        }
    }
}