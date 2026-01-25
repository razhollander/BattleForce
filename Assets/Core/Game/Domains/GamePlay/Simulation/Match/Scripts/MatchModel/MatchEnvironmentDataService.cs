using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchEnvironmentDataService
    {
        public Vector2 EnvironmentHalfSize { get; private set; }
        public TalentCardS2C[] TalentCards { get; private set; }
        public WallConfig[] LavaWallConfigs { get; private set; }
        public WallConfig[] WallConfigs { get; private set; }
        
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        
        public MatchEnvironmentDataService(SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void InitEntryPoint(int environmentLayoutIndex)
        {
            WallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetWalls();
            LavaWallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetLavaWalls();
            TalentCards = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetTalentCards();
            EnvironmentHalfSize = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetEnvironmentHalfSize();
        }
    }
}