using System;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct ObjectLockedOnTargetS2C : IComparable<ObjectLockedOnTargetS2C>
    {
        public const int NO_RETENTION_END_TICK = 0;

        public ushort TargetId;
        public bool IsLockOnTargetShootable;
        public LockOnTargetType TargetType;
        public int RetentionEndTick;

        public bool IsLockOnTargetRetained => RetentionEndTick != NO_RETENTION_END_TICK;

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
