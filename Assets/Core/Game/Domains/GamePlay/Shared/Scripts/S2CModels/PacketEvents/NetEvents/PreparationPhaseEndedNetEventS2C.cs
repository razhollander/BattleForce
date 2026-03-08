using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PreparationPhaseEndedNetEventS2C : INetSerializable, IComparable<PreparationPhaseEndedNetEventS2C>
    {
        public int OccuredOnTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
        }

        public int CompareTo(PreparationPhaseEndedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
