using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct EndPowerUpGrantingPhaseNetEventS2C : INetSerializable, IComparable<EndPowerUpGrantingPhaseNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public PowerUpType GrantedPowerUp;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.Put((byte)GrantedPowerUp);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            GrantedPowerUp = (PowerUpType)reader.GetByte();
        }

        public int CompareTo(EndPowerUpGrantingPhaseNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
