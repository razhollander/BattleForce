using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct ChickenEggHitNetEventS2C : INetSerializable, IComparable<ChickenEggHitNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort EggId;
        public ushort HitByPlayerId;

        public ChickenEggHitNetEventS2C(int occuredOnTick, ushort eggId, ushort hitByPlayerId)
        {
            OccuredOnTick = occuredOnTick;
            EggId = eggId;
            HitByPlayerId = hitByPlayerId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(EggId);
            writer.Put((byte)HitByPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            EggId = reader.GetUShort();
            HitByPlayerId = reader.GetByte();
        }

        public int CompareTo(ChickenEggHitNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
