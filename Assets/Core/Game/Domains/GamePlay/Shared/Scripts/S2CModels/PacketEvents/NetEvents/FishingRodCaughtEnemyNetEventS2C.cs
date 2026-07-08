using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct FishingRodCaughtEnemyNetEventS2C : INetSerializable, IComparable<FishingRodCaughtEnemyNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ProjectileId;
        public ushort CasterPlayerId;
        public ushort CaughtEnemyId;

        public FishingRodCaughtEnemyNetEventS2C(int occuredOnTick, ushort projectileId, ushort casterPlayerId, ushort caughtEnemyId)
        {
            OccuredOnTick = occuredOnTick;
            ProjectileId = projectileId;
            CasterPlayerId = casterPlayerId;
            CaughtEnemyId = caughtEnemyId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(ProjectileId);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)CaughtEnemyId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            CaughtEnemyId = reader.GetByte();
        }

        public int CompareTo(FishingRodCaughtEnemyNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
