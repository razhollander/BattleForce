using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    [Serializable]
    public struct PowerUpBallS2C : INetSerializable
    {
        public ushort Id;
        public Vector2 Position;
        public PowerUpType Type;
        public Vector2 Velocity; // server only

        public PowerUpBallS2C(ushort id, Vector2 position, PowerUpType type, Vector2 velocity)
        {
            Id = id;
            Position = position;
            Type = type;
            Velocity = velocity;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.Put((byte)Type);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            Type = (PowerUpType)reader.GetByte();
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
    }
}