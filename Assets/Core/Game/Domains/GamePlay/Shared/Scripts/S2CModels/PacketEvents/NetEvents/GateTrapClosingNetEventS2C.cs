using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct GateTrapClosingNetEventS2C : INetSerializable, IComparable<GateTrapClosingNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort GateTrapId;
        public int FinishClosingOnTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)GateTrapId);
            writer.Put(FinishClosingOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            GateTrapId = reader.GetByte();
            FinishClosingOnTick = reader.GetInt();
        }

        public int CompareTo(GateTrapClosingNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
