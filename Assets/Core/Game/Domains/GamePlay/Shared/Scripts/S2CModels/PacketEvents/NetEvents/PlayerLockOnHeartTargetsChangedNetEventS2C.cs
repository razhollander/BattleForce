using System;
using LiteNetLib.Utils;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class PlayerLockOnHeartTargetsChangedNetEventS2C : INetSerializable, IComparable<PlayerLockOnHeartTargetsChangedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public FixedUnorderedList<ushort> PlayerIdsLockedOnTarget;

        public PlayerLockOnHeartTargetsChangedNetEventS2C(int maxTargets)
        {
            PlayerIdsLockedOnTarget = new FixedUnorderedList<ushort>(maxTargets);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.Put((byte)PlayerIdsLockedOnTarget.Count);
            foreach (var target in PlayerIdsLockedOnTarget.AsSpan())
            {
                writer.Put((byte)target);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            PlayerIdsLockedOnTarget.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var target = ref PlayerIdsLockedOnTarget.AddAndGet();
                target = reader.GetByte();
            }
        }

        public int CompareTo(PlayerLockOnHeartTargetsChangedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
