using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerPowerUpChangedNetEventS2C : INetSerializable, IComparable<PlayerPowerUpChangedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public PowerUpType PowerUp;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.Put((byte)PowerUp);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            PowerUp = (PowerUpType)reader.GetByte();
        }

        public int CompareTo(PlayerPowerUpChangedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
