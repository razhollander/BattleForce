using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateChickenTalentNetEventS2C : INetSerializable, IComparable<DeactivateChickenTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public int CooldownEndTick;

        public DeactivateChickenTalentNetEventS2C(int occuredOnTick, ushort casterPlayerId, int cooldownEndTick)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            CooldownEndTick = cooldownEndTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put(CooldownEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            CooldownEndTick = reader.GetInt();
        }

        public int CompareTo(DeactivateChickenTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
