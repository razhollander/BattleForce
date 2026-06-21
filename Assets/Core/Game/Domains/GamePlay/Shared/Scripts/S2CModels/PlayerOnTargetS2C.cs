using System;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerOnTargetS2C : IComparable<PlayerOnTargetS2C>, IEquatable<ushort>
    {
        public ushort PlayerTargetId;
        public bool IsLockOnTargetShootable;

        public int CompareTo(PlayerOnTargetS2C other)
        {
            return PlayerTargetId.CompareTo(other.PlayerTargetId);
        }

        public bool Equals(ushort otherId)
        {
            return PlayerTargetId == otherId;
        }
    }
}
