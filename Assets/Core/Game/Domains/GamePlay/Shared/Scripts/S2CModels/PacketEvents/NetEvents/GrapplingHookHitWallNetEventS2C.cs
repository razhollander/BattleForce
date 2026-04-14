using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct GrapplingHookHitWallNetEventS2C : INetSerializable, IComparable<GrapplingHookHitWallNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ProjectileId;
        public ushort HitWallId;
        public Vector2 HitPosition;

        public GrapplingHookHitWallNetEventS2C(int occuredOnTick, ushort projectileId, ushort hitWallId, Vector2 hitPosition)
        {
            OccuredOnTick = occuredOnTick;
            ProjectileId = projectileId;
            HitWallId = hitWallId;
            HitPosition = hitPosition;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(ProjectileId);
            writer.Put(HitWallId);
            writer.PutVector2Quantized(HitPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            HitWallId = reader.GetUShort();
            HitPosition = reader.GetVector2Quantized();
        }

        public int CompareTo(GrapplingHookHitWallNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
