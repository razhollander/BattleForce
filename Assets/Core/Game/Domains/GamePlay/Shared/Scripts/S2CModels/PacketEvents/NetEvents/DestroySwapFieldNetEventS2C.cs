using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct DestroySwapFieldNetEventS2C : INetSerializable, IComparable<DestroySwapFieldNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;

        public DestroySwapFieldNetEventS2C(int occuredOnTick, ushort casterPlayerId)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(CasterPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetUShort();
        }

        public int CompareTo(DestroySwapFieldNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
