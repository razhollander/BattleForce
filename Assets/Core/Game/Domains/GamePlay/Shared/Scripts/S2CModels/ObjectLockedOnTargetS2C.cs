using System;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct ObjectLockedOnTargetS2C : IComparable<ObjectLockedOnTargetS2C>
    {
        public ushort TargetId;
        public bool IsLockOnTargetShootable;
        public LockOnTargetType TargetType;

        public LockOnTargetKey GetKey()
        {
            return new LockOnTargetKey(TargetId, TargetType);
        }

        public int CompareTo(ObjectLockedOnTargetS2C other)
        {
            var targetTypeComparison = TargetType.CompareTo(other.TargetType);
            return targetTypeComparison != 0 ? targetTypeComparison : TargetId.CompareTo(other.TargetId);
        }
    }
}
