using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct DestroyChickenEggNetEventS2C : INetSerializable, IComparable<DestroyChickenEggNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort EggId;

        public DestroyChickenEggNetEventS2C(int occuredOnTick, ushort eggId)
        {
            OccuredOnTick = occuredOnTick;
            EggId = eggId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(EggId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            EggId = reader.GetUShort();
        }

        public int CompareTo(DestroyChickenEggNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
