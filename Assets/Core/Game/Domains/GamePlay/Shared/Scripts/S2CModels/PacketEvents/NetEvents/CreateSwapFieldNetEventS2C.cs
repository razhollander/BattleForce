using System;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateSwapFieldNetEventS2C : INetSerializable, IComparable<CreateSwapFieldNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort SwapFieldId;
        public ushort CasterPlayerId;
        public int EndOnTick;
        public float MaxRadius;

        public CreateSwapFieldNetEventS2C(int occuredOnTick, ushort swapFieldId, ushort casterPlayerId, int endOnTick, float maxRadius)
        {
            OccuredOnTick = occuredOnTick;
            SwapFieldId = swapFieldId;
            CasterPlayerId = casterPlayerId;
            EndOnTick = endOnTick;
            MaxRadius = maxRadius;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(SwapFieldId);
            writer.Put((byte)CasterPlayerId);
            writer.Put(EndOnTick);
            writer.PutFloat16(MaxRadius);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            SwapFieldId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            EndOnTick = reader.GetInt();
            MaxRadius = reader.GetFloat16();
        }

        public int CompareTo(CreateSwapFieldNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
