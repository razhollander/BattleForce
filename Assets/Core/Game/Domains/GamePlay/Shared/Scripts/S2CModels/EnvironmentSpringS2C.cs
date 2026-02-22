using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct EnvironmentSpringS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public Vector2 Position;
        public float DirectionAngle;

        public EnvironmentSpringS2C(ushort id, Vector2 position, float directionAngle)
        {
            Id = id;
            Position = position;
            DirectionAngle = directionAngle;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(DirectionAngle);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            DirectionAngle = reader.GetFloat16();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
