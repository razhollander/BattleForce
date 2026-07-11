using System;
using LiteNetLib.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class PlayerLockOnTargetsChangedNetEventS2C : INetSerializable, IComparable<PlayerLockOnTargetsChangedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public FixedUnorderedList<ObjectLockedOnTargetS2C> LockedOnTargetObjects;

        public PlayerLockOnTargetsChangedNetEventS2C(int maxTargets)
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
                if (target.TargetId > 255)
                {
                    LogService.LogError($"TargetId bigger than 255!, target type {target.TargetType}, id: {target.TargetId}");
                }
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

        public int CompareTo(PlayerLockOnTargetsChangedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
