using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct HeadbuttHitEnemyNetEventS2C : INetSerializable, IComparable<HeadbuttHitEnemyNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public ushort EnemyPlayerId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)EnemyPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            EnemyPlayerId = reader.GetByte();
        }

        public int CompareTo(HeadbuttHitEnemyNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
