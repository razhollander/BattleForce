using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateSwapFieldNetEventS2C : INetSerializable, IComparable<CreateSwapFieldNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort SwapFieldId;
        public ushort CasterPlayerId;
        public int EndOnTick;

        public CreateSwapFieldNetEventS2C(int occuredOnTick, ushort swapFieldId, ushort casterPlayerId, int endOnTick)
        {
            OccuredOnTick = occuredOnTick;
            SwapFieldId = swapFieldId;
            CasterPlayerId = casterPlayerId;
            EndOnTick = endOnTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(SwapFieldId);
            writer.Put((byte)CasterPlayerId);
            writer.Put(EndOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            SwapFieldId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            EndOnTick = reader.GetInt();
        }

        public int CompareTo(CreateSwapFieldNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
