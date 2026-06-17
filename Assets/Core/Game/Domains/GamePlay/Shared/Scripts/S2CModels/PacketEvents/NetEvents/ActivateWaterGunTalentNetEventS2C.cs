using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct ActivateWaterGunTalentNetEventS2C : INetSerializable, IComparable<ActivateWaterGunTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;

        public ActivateWaterGunTalentNetEventS2C(int occuredOnTick, ushort casterPlayerId)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
        }

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

        public int CompareTo(ActivateWaterGunTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
