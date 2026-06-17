using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PerformHeadbuttDashNetEventS2C : INetSerializable, IComparable<PerformHeadbuttDashNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
        }

        public int CompareTo(PerformHeadbuttDashNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
