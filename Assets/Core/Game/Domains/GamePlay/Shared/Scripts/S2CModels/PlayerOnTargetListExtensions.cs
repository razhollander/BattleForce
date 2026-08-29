using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public static class PlayerOnTargetListExtensions
    {
        public static bool ContainsTarget(this FixedUnorderedList<ObjectLockedOnTargetS2C> targets, LockOnTargetKey targetKey)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].GetKey().Equals(targetKey))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAnyNonRetainedTarget(this FixedUnorderedList<ObjectLockedOnTargetS2C> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].IsLockOnTargetRetained)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsIdentical(this FixedUnorderedList<ObjectLockedOnTargetS2C> listA, FixedUnorderedList<ObjectLockedOnTargetS2C> listB)
        {
            if (listA.Count != listB.Count)
            {
                return false;
            }

            for (int i = 0; i < listA.Count; i++)
            {
                var playerOnTargetA = listA[i];
                var playerOnTargetB = listB[i];
                var isDifferent = playerOnTargetA.TargetId != playerOnTargetB.TargetId || playerOnTargetA.TargetType != playerOnTargetB.TargetType ||
                                  playerOnTargetA.IsLockOnTargetShootable != playerOnTargetB.IsLockOnTargetShootable || playerOnTargetA.RetentionEndTick != playerOnTargetB.RetentionEndTick;
                if (isDifferent)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
