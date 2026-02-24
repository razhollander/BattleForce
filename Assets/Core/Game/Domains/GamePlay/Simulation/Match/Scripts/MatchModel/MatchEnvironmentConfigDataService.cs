using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchEnvironmentConfigDataService : IMatchEnvironmentConfigDataService
    {
        public Vector2 EnvironmentHalfSize { get; private set; }
        public TalentCardConfig[] TalentCards { get; private set; }
        public EnvironmentSpringConfig[] EnvironmentSprings { get; private set; }
        public EnvironmentTeleportGatePairConfig[] TeleportGates { get; private set; }
        public WallConfig[] LavaWallConfigs { get; private set; }
        public WallConfig[] WallConfigs { get; private set; }
        public EnvironmentRotatingWheelConfig[] RotatingWheels { get; private set; }
        public EnvironmentFieldBarrierConfig[] FieldBarrierConfigs { get; private set; }

        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        
        public MatchEnvironmentConfigDataService(SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }
        
        public void InitEnvironmentLayout(int environmentLayoutIndex)
        {
            WallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetWalls();
            LavaWallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetLavaWalls();
            TalentCards = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetTalentCards();
            EnvironmentSprings = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetEnvironmentSprings();
            TeleportGates = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetTeleportGates();
            RotatingWheels = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetRotatingWheels();
            EnvironmentHalfSize = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetEnvironmentHalfSize();
            FieldBarrierConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetFieldBarriers();
        }
    }
}