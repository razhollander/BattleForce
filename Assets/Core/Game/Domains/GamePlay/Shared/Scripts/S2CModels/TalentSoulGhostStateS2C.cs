using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentSoulGhostStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort PlayerCasterId;
        public Vector2 Position;
        public Vector2 Direction;
        public Vector2 Velocity;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put((byte)PlayerCasterId);
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Direction);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            PlayerCasterId = reader.GetByte();
            Position = reader.GetVector2Quantized();
            Direction = reader.GetVector2FromAngle16();
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
