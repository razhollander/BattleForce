using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct ShootFrigidBlockNetEventS2C : INetSerializable, IComparable<ShootFrigidBlockNetEventS2C>
    {
        public int OccuredOnTick;
        public TalentFrigidBlockStateS2C FrigidBlock;
        public int CooldownEndTick;

        public ShootFrigidBlockNetEventS2C(int occuredOnTick, TalentFrigidBlockStateS2C frigidBlock, int cooldownEndTick)
        {
            OccuredOnTick = occuredOnTick;
            FrigidBlock = frigidBlock;
            CooldownEndTick = cooldownEndTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(FrigidBlock);
            writer.Put(CooldownEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            FrigidBlock.Deserialize(reader);
            CooldownEndTick = reader.GetInt();
        }

        public int CompareTo(ShootFrigidBlockNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
