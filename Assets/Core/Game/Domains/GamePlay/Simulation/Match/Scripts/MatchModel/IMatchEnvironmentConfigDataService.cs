using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public interface IMatchEnvironmentConfigDataService
    {
        Vector2 EnvironmentHalfSize { get; }
        TalentCardConfig[] TalentCards { get; }
        EnvironmentSpringConfig[] EnvironmentSprings { get; }
        EnvironmentSpikeConfig[] EnvironmentSpikes { get; }
        EnvironmentTeleportGatePairConfig[] TeleportGates { get; }
        WallConfig[] LavaWallConfigs { get; }
        WallConfig[] StageBoundaries { get; }
        WallConfig[] WallConfigs { get; }
        EnvironmentRotatingWheelConfig[] RotatingWheels { get; }
        EnvironmentFieldBarrierConfig[] FieldBarrierConfigs { get; }
        void InitEnvironmentLayout(int environmentLayoutId);
    }
}