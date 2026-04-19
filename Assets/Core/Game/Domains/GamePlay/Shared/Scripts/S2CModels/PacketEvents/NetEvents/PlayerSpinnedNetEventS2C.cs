using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerSpinnedNetEventS2C : INetSerializable, IComparable<PlayerSpinnedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public int SpinEndOnTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PlayerId);
            writer.Put(SpinEndOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetUShort();
            SpinEndOnTick = reader.GetInt();
        }

        public int CompareTo(PlayerSpinnedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
