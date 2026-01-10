using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [Serializable]
    public struct TalentCard : INetSerializable
    {
        public Vector2 Position;
        public TalentType TalentType;

        public TalentCard(Vector2 position, TalentType talentType)
        {
            Position = position;
            TalentType = talentType;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Position.X);
            writer.Put(Position.Y);
            writer.Put((int)TalentType);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = new Vector2(reader.GetFloat(), reader.GetFloat());
            TalentType = (TalentType)reader.GetInt();
        }
    }
}
