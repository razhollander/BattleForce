using System;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct ObjectLockedOnTargetS2C : IComparable<ObjectLockedOnTargetS2C>, IEquatable<ushort>
    {
        public ushort PlayerTargetId;
        public bool IsLockOnTargetShootable;
        public LockOnTargetType TargetType;

        public int CompareTo(ObjectLockedOnTargetS2C other)
        {
            return PlayerTargetId.CompareTo(other.PlayerTargetId);
        }

        public bool Equals(ushort otherId)
        {
            return PlayerTargetId == otherId;
        }
    }
}
