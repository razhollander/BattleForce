using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerEndedExposedToLavaNetEventS2C : INetSerializable, IComparable<PlayerEndedExposedToLavaNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
        }

        public int CompareTo(PlayerEndedExposedToLavaNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
