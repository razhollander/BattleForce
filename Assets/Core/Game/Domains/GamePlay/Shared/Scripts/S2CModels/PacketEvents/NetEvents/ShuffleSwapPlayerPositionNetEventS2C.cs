using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct ShuffleSwapPlayerPositionNetEventS2C : INetSerializable, IComparable<ShuffleSwapPlayerPositionNetEventS2C>
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

        public int CompareTo(ShuffleSwapPlayerPositionNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
