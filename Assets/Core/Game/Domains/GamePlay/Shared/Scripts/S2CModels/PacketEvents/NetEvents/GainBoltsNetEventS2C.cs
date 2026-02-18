using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct GainBoltsNetEventS2C : INetSerializable, IComparable<GainBoltsNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public int GainedAmount;
        public int TotalTeamBolts;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PlayerId);
            writer.Put(GainedAmount);
            writer.Put(TotalTeamBolts);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetUShort();
            GainedAmount = reader.GetInt();
            TotalTeamBolts = reader.GetInt();
        }

        public int CompareTo(GainBoltsNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
