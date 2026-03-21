using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateSwapTalentNetEventS2C : INetSerializable, IComparable<DeactivateSwapTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public ushort SwapFieldId;
        public int TalentCooldownEndTick;

        public DeactivateSwapTalentNetEventS2C(int occuredOnTick, ushort casterPlayerId, ushort swapFieldId, int talentCooldownEndTick)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            SwapFieldId = swapFieldId;
            TalentCooldownEndTick = talentCooldownEndTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put(SwapFieldId);
            writer.Put(TalentCooldownEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            SwapFieldId = reader.GetUShort();
            TalentCooldownEndTick = reader.GetInt();
        }

        public int CompareTo(DeactivateSwapTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
