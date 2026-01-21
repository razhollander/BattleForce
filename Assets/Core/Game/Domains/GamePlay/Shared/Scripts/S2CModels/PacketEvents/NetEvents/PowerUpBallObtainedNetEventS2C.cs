using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct PowerUpBallObtainedNetEventS2C : INetSerializable, IComparable<PowerUpBallObtainedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort Id;
        public ushort ObtainedByPlayerId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(Id);
            writer.Put((byte)ObtainedByPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            Id = reader.GetUShort();
            ObtainedByPlayerId = reader.GetByte();
        }

        public int CompareTo(PowerUpBallObtainedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
