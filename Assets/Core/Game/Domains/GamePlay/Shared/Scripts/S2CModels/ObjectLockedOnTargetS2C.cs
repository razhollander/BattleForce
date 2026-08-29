using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct ObjectLockedOnTargetS2C : IComparable<ObjectLockedOnTargetS2C>, INetSerializable
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

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)TargetId);
            writer.Put(IsLockOnTargetShootable);
            writer.Put((byte)TargetType);
            writer.Put(RetentionEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            TargetId = reader.GetByte();
            IsLockOnTargetShootable = reader.GetBool();
            TargetType = (LockOnTargetType)reader.GetByte();
            RetentionEndTick = reader.GetInt();
        }
    }
}
