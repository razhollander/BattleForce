using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    [Serializable]
    public struct PowerUpS2C : INetSerializable
    {
        public ushort Id;
        public Vector2 Position;
        public PowerUpType Type;
        public Vector2 Direction;
        public float Radius;

        public PowerUpS2C(ushort id, Vector2 position, PowerUpType type, Vector2 direction, float radius)
        {
            Id = id;
            Position = position;
            Type = type;
            Direction = direction;
            Radius = radius;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(Position);
            writer.Put((byte)Type);
            writer.Put(Direction);
            writer.Put(Radius);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2();
            Type = (PowerUpType)reader.GetByte();
            Direction = reader.GetVector2();
            Radius = reader.GetFloat();
        }

        public void SerializeTransforms(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(Position);
        }

        public void DeserializeTransforms(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2();
        }
    }
}
