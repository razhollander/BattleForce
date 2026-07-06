using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct ActivateShuffleNetEventS2C : INetSerializable, IComparable<ActivateShuffleNetEventS2C>
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

        public int CompareTo(ActivateShuffleNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
