using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct ActivateHeadbuttChargingNetEventS2C : INetSerializable, IComparable<ActivateHeadbuttChargingNetEventS2C>
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

        public int CompareTo(ActivateHeadbuttChargingNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
