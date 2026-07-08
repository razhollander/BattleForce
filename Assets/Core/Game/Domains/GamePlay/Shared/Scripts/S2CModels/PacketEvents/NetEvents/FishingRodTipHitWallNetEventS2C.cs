using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct FishingRodTipHitWallNetEventS2C : INetSerializable, IComparable<FishingRodTipHitWallNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ProjectileId;
        public Vector2 HitPosition;

        public FishingRodTipHitWallNetEventS2C(int occuredOnTick, ushort projectileId, Vector2 hitPosition)
        {
            OccuredOnTick = occuredOnTick;
            ProjectileId = projectileId;
            HitPosition = hitPosition;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(ProjectileId);
            writer.PutVector2Quantized(HitPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            HitPosition = reader.GetVector2Quantized();
        }

        public int CompareTo(FishingRodTipHitWallNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
