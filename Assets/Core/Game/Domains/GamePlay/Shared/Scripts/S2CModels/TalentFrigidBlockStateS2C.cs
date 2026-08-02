using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentFrigidBlockStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort PlayerCasterId;
        public Vector2 Position;
        public Vector2 Rotation; // unit direction vector representing the block's facing
        // Server-only fields (not serialized) used for idle detection.
        public Vector2 Velocity;
        public float AngularVelocity;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(PlayerCasterId);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(Rotation.X);
            writer.PutFloat16(Rotation.Y);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            PlayerCasterId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            var rotationX = reader.GetFloat16();
            var rotationY = reader.GetFloat16();
            Rotation = new Vector2(rotationX, rotationY);
        }

        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(Rotation.X);
            writer.PutFloat16(Rotation.Y);
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            var rotationX = reader.GetFloat16();
            var rotationY = reader.GetFloat16();
            Rotation = new Vector2(rotationX, rotationY);
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
