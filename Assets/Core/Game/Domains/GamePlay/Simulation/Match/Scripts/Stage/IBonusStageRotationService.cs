using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public interface IBonusStageRotationService
    {
        // Advances the rotation and returns the bonus stage type to play now. Never returns the same type twice in a
        // row (unless only one is enabled), so with both types enabled the bonus stages strictly alternate.
        StageType ResolveNextBonusStageType();

        // Called once per match so the first bonus stage is chosen freshly at random.
        void ResetData();
    }
}
