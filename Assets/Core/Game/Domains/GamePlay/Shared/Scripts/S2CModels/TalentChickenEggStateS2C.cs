using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    [Serializable]
    public struct TalentChickenEggStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort PlayerCasterId;
        public Vector2 Position;
        public bool IsBroken;
        public int BrokenTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(PlayerCasterId);
            writer.PutVector2Quantized(Position);
            writer.Put(IsBroken);
            writer.Put(BrokenTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            PlayerCasterId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            IsBroken = reader.GetBool();
            BrokenTick = reader.GetInt();
        }

        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(IsBroken);
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetUShort();
            IsBroken = reader.GetBool();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
