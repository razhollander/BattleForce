using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct DestroyFrigidBlockNetEventS2C : INetSerializable, IComparable<DestroyFrigidBlockNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort BlockId;

        public DestroyFrigidBlockNetEventS2C(int occuredOnTick, ushort blockId)
        {
            OccuredOnTick = occuredOnTick;
            BlockId = blockId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(BlockId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            BlockId = reader.GetUShort();
        }

        public int CompareTo(DestroyFrigidBlockNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
