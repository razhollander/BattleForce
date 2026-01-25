using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerSwitchTeamNetEventS2C : INetSerializable, IComparable<PlayerSwitchTeamNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public ushort TeamId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PlayerId);
            writer.Put((byte)TeamId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetUShort();
            TeamId = reader.GetByte();
        }

        public int CompareTo(PlayerSwitchTeamNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
