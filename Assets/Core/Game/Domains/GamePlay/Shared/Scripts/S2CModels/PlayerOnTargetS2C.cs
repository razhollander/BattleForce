using System;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerOnTargetS2C : IComparable<PlayerOnTargetS2C>
    {
        public ushort PlayerTargetId;
        public bool IsLockOnTargetShootable;

        public int CompareTo(PlayerOnTargetS2C other)
        {
            return PlayerTargetId.CompareTo(other.PlayerTargetId);
        }
    }
}
