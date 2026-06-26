using System;
using LiteNetLib.Utils;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class PlayerLockOnHeartTargetsChangedNetEventS2C : INetSerializable, IComparable<PlayerLockOnHeartTargetsChangedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public FixedUnorderedList<ObjectLockedOnTargetS2C> LockedOnTargetObjects;

        public PlayerLockOnHeartTargetsChangedNetEventS2C(int maxTargets)
        {
            LockedOnTargetObjects = new FixedUnorderedList<ObjectLockedOnTargetS2C>(maxTargets);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.Put((byte)LockedOnTargetObjects.Count);
            foreach (var target in LockedOnTargetObjects.AsSpan())
            {
                writer.Put((byte)target.TargetId);
                writer.Put(target.IsLockOnTargetShootable);
                writer.Put((byte)target.TargetType);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            LockedOnTargetObjects.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var target = ref LockedOnTargetObjects.AddAndGet();
                target.TargetId = reader.GetByte();
                target.IsLockOnTargetShootable = reader.GetBool();
                target.TargetType = (LockOnTargetType)reader.GetByte();
            }
        }

        public int CompareTo(PlayerLockOnHeartTargetsChangedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
