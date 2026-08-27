using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public interface ILockOnTargetRetentionService
    {
        void AddRetainedTargets(PlayerStateS2C casterPlayerState, FixedUnorderedList<ObjectLockedOnTargetS2C> outputTargetedObjects, int processedTick);
    }
}
