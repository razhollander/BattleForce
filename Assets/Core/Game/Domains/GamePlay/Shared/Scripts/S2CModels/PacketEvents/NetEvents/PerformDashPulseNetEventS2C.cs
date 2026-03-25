using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PerformDashPulseNetEventS2C : INetSerializable, IComparable<PerformDashPulseNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public int RemainingDashPulseStocksAmount;

        public PerformDashPulseNetEventS2C(int occuredOnTick, ushort casterPlayerId, int remainingDashPulseStocksAmount)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            RemainingDashPulseStocksAmount = remainingDashPulseStocksAmount;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)RemainingDashPulseStocksAmount);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            RemainingDashPulseStocksAmount = reader.GetByte();
        }

        public int CompareTo(PerformDashPulseNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
