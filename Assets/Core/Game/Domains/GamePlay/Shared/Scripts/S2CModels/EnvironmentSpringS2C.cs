using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct EnvironmentSpringS2C : INetSerializable
    {
        public ushort Id;
        public Vector2 Position;
        public float Rotation;

        public EnvironmentSpringS2C(ushort id, Vector2 position, float rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.Put(Rotation);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            Rotation = reader.GetFloat();
        }
    }
}
