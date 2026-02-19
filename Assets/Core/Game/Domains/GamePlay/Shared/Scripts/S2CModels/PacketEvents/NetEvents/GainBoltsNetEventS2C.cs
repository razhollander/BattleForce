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
            writer.Put((byte)PlayerId);
            writer.Put((ushort)GainedAmount);
            writer.Put((ushort)TotalTeamBolts);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            GainedAmount = reader.GetUShort();
            TotalTeamBolts = reader.GetUShort();
        }

        public int CompareTo(GainBoltsNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
