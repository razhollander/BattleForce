using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentGrapplingHookProjectileStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort PlayerCasterId;
        public Vector2 StartPosition;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IsAttached;
        public ushort AttachedWallId;
        public Vector2 AttachedLocalPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(PlayerCasterId);
            writer.PutVector2Quantized(Position);
            writer.Put(IsAttached);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            PlayerCasterId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            IsAttached = reader.GetBool();
        }

        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
