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
        public PowerUpSpawnPointConfig[] PowerUpSpawnPoints { get; private set; }
        public MoleSpawnPointConfig[] MoleSpawnPoints { get; private set; }

        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        
        public MatchEnvironmentConfigDataService(SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }
        
        public void InitEnvironmentLayout(int environmentLayoutId)
        {
            var environmentLayoutConfig = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutId);
            WallConfigs = environmentLayoutConfig.GetWalls();
            LavaWallConfigs = environmentLayoutConfig.GetLavaWalls();
            StageBoundaries = environmentLayoutConfig.GetStageBoundaries();
            TalentCards = environmentLayoutConfig.GetTalentCards();
            EnvironmentSprings = environmentLayoutConfig.GetEnvironmentSprings();
            EnvironmentSpikes = environmentLayoutConfig.GetEnvironmentSpikes();
            TeleportGates = environmentLayoutConfig.GetTeleportGates();
            RotatingWheels = environmentLayoutConfig.GetRotatingWheels();
            EnvironmentHalfSize = environmentLayoutConfig.GetEnvironmentHalfSize();
            FieldBarrierConfigs = environmentLayoutConfig.GetFieldBarriers();
            PowerUpSpawnPoints = environmentLayoutConfig.GetPowerUpSpawnPoints();
            MoleSpawnPoints = environmentLayoutConfig.GetMoleSpawnPoints();
        }
    }
}