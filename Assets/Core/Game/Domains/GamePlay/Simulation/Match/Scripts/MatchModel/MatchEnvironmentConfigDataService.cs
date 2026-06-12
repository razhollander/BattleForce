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
        public EnvironmentSpikeConfig[] EnvironmentSpikes { get; private set; }
        public EnvironmentTeleportGatePairConfig[] TeleportGates { get; private set; }
        public WallConfig[] LavaWallConfigs { get; private set; }
        public WallConfig[] StageBoundaries { get; private set; }
        public WallConfig[] WallConfigs { get; private set; }
        public EnvironmentRotatingWheelConfig[] RotatingWheels { get; private set; }
        public EnvironmentFieldBarrierConfig[] FieldBarrierConfigs { get; private set; }

        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        
        public MatchEnvironmentConfigDataService(SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }
        
        public void InitEnvironmentLayout(int environmentLayoutId)
        {
            WallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetWalls();
            LavaWallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetLavaWalls();
            StageBoundaries = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetStageBoundaries();
            TalentCards = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetTalentCards();
            EnvironmentSprings = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetEnvironmentSprings();
            EnvironmentSpikes = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetEnvironmentSpikes();
            TeleportGates = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetTeleportGates();
            RotatingWheels = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetRotatingWheels();
            EnvironmentHalfSize = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetEnvironmentHalfSize();
            FieldBarrierConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId).GetFieldBarriers();
        }
    }
}