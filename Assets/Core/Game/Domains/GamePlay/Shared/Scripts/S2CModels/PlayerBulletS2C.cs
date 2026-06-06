using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerBulletS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Direction;
        public float Radius;
        public int CreatedOnTick; // only server

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put((byte)BelongToPlayerId);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(Radius);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            BelongToPlayerId = reader.GetByte();
            Position = reader.GetVector2Quantized();
            Radius = reader.GetFloat16();
        }

        public void SerializeTransforms(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
        }

        public void DeserializeTransforms(NetDataReader reader)
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