using System;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    /// <summary>
    /// Identity of a lock-on target. A target is unique by the combination of TargetId and TargetType,
    /// because different object types (e.g. a PowerUpBall and an enemy Player) can share the same id.
    /// </summary>
    public readonly struct LockOnTargetKey : IEquatable<LockOnTargetKey>
    {
        public readonly ushort TargetId;
        public readonly LockOnTargetType TargetType;

        public LockOnTargetKey(ushort targetId, LockOnTargetType targetType)
        {
            TargetId = targetId;
            TargetType = targetType;
        }

        public bool Equals(LockOnTargetKey other)
        {
            return TargetId == other.TargetId && TargetType == other.TargetType;
        }

        public override bool Equals(object obj)
        {
            return obj is LockOnTargetKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (TargetId << 8) ^ (int)TargetType;
        }
    }
}
