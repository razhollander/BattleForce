using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;


namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentKOProjectileS2C : INetSerializable, IEquatable<ushort>
    {
        public int CreatedOnTick;
        public ushort Id;
        public ushort PlayerCasterId;
        public Vector2 Position;
        public Vector2 Rotation;
        public Vector2 Velocity;
        public float Size;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(PlayerCasterId);
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Rotation);
            writer.PutFloat16(Size);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            PlayerCasterId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            Rotation = reader.GetVector2FromAngle16();
            Size = reader.GetFloat16();
        }

        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Rotation);
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            Rotation = reader.GetVector2FromAngle16();
        }
        
        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}