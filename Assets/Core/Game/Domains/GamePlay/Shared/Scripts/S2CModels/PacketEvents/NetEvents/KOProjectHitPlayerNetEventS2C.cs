using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct KOProjectHitPlayerNetEventS2C : INetSerializable, IComparable<KOProjectHitPlayerNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort KoProjectileId;
        public Vector2 HitPoint;

        public KOProjectHitPlayerNetEventS2C(int occuredOnTick, ushort koProjectileId, Vector2 hitPoint)
        {
            OccuredOnTick = occuredOnTick;
            KoProjectileId = koProjectileId;
            HitPoint = hitPoint;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(KoProjectileId);
            writer.PutVector2Quantized(HitPoint);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            KoProjectileId = reader.GetUShort();
            HitPoint = reader.GetVector2Quantized();
        }

        public int CompareTo(KOProjectHitPlayerNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
