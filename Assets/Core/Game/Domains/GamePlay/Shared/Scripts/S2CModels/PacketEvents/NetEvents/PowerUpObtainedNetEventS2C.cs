using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct PowerUpObtainedNetEventS2C : INetSerializable, IComparable<PowerUpObtainedNetEventS2C>
    {
        public ushort PowerUpId;
        public ushort PlayerId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PowerUpId);
            writer.Put((byte)PlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            PowerUpId = reader.GetUShort();
            PlayerId = reader.GetByte();
        }

        public int CompareTo(PowerUpObtainedNetEventS2C other)
        {
            return PowerUpId.CompareTo(other.PowerUpId);
        }
    }
}
