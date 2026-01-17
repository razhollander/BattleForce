using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct TalentCardHitNetEventS2C : INetSerializable, IComparable<TalentCardHitNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort TalentCardId;
        public ushort TalentCardHealth;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(TalentCardId);
            writer.Put((byte)TalentCardHealth);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            TalentCardId = reader.GetUShort();
            TalentCardHealth = reader.GetByte();
        }

        public int CompareTo(TalentCardHitNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}