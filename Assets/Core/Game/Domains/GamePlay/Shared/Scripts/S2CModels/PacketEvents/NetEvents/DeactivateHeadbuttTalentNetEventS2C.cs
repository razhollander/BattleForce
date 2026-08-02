using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateHeadbuttTalentNetEventS2C : INetSerializable, IComparable<DeactivateHeadbuttTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public int TalentCooldownEndTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put(TalentCooldownEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            TalentCooldownEndTick = reader.GetInt();
        }

        public int CompareTo(DeactivateHeadbuttTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
