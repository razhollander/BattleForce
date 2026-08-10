using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    /// <summary>
    /// The single event a gate trap ever sends. Everything after it - staying closed, opening again and the open
    /// cooldown - is derived on the client from the authored durations, so the trap costs one event per cycle.
    /// </summary>
    [Serializable]
    public struct GateTrapClosingNetEventS2C : INetSerializable, IComparable<GateTrapClosingNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort GateTrapId;

        // Tick the wall reaches its closed pose on; the client interpolates towards it until then.
        public int ClosedOnTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)GateTrapId);
            writer.Put(ClosedOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            GateTrapId = reader.GetByte();
            ClosedOnTick = reader.GetInt();
        }

        public int CompareTo(GateTrapClosingNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
