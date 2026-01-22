using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentCardS2C : INetSerializable
    {
        public ushort Id;
        public Vector2 Position;
        public TalentType TalentType;
        public ushort Health;

        public TalentCardS2C(ushort Id, Vector2 position, TalentType talentType, ushort health)
        {
            this.Id = Id;
            Position = position;
            TalentType = talentType;
            Health = health;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.PutVector2Quantized(Position);
            writer.Put((byte)TalentType);
            writer.Put(Health);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2Quantized();
            TalentType = (TalentType)reader.GetByte();
            Health = reader.GetUShort();
        }
    }
}