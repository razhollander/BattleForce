using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateChickenTalentNetEventS2C : INetSerializable, IComparable<DeactivateChickenTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public int TalentCooldownEndTick;

        public DeactivateChickenTalentNetEventS2C(int occuredOnTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            TalentCooldownEndTick = talentCooldownEndTick;
        }

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

        public int CompareTo(DeactivateChickenTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
