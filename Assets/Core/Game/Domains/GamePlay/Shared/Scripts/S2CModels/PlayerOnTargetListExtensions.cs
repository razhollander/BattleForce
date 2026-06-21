using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public static class PlayerOnTargetListExtensions
    {
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
                var isDifferent = playerOnTargetA.PlayerTargetId != playerOnTargetB.PlayerTargetId || playerOnTargetA.IsLockOnTargetShootable != playerOnTargetB.IsLockOnTargetShootable;
                if (isDifferent)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
