using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentGrapplingHookProjectileS2C : INetSerializable, IEquatable<ushort>
    {
        public int CreatedOnTick;
        public ushort Id;
        public ushort PlayerCasterId;
        public Vector2 StartPosition;
        public Vector2 Position;
        public Vector2 Rotation;
        public Vector2 Velocity;
        public float Size;
        public bool IsAttached;
        public ushort AttachedWallId;
        public Vector2 AttachedLocalPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(PlayerCasterId);
            writer.PutVector2Quantized(StartPosition);
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Rotation);
            writer.PutFloat16(Size);
            writer.Put(IsAttached);
            if (IsAttached)
            {
                writer.Put(AttachedWallId);
                writer.PutVector2Quantized(AttachedLocalPosition);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            PlayerCasterId = reader.GetUShort();
            StartPosition = reader.GetVector2Quantized();
            Position = reader.GetVector2Quantized();
            Rotation = reader.GetVector2FromAngle16();
            Size = reader.GetFloat16();
            IsAttached = reader.GetBool();
            if (IsAttached)
            {
                AttachedWallId = reader.GetUShort();
                AttachedLocalPosition = reader.GetVector2Quantized();
            }
        }

        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Rotation);
            writer.Put(IsAttached);
            if (IsAttached)
            {
                writer.Put(AttachedWallId);
                writer.PutVector2Quantized(AttachedLocalPosition);
            }
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            Rotation = reader.GetVector2FromAngle16();
            IsAttached = reader.GetBool();
            if (IsAttached)
            {
                AttachedWallId = reader.GetUShort();
                AttachedLocalPosition = reader.GetVector2Quantized();
            }
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
