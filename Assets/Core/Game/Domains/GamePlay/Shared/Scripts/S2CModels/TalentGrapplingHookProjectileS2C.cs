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
        public Vector2 Position;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(PlayerCasterId);
            writer.PutVector2Quantized(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            PlayerCasterId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
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
