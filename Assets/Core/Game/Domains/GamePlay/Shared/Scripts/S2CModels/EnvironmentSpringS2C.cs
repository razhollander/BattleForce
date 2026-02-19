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
        public float RotationAngle;

        public EnvironmentSpringS2C(ushort id, Vector2 position, float rotationAngle)
        {
            Id = id;
            Position = position;
            RotationAngle = rotationAngle;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.Put(RotationAngle);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            RotationAngle = reader.GetFloat();
        }
    }
}
