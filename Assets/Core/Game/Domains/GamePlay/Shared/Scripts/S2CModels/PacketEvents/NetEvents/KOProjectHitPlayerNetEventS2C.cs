using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct KOProjectHitPlayerNetEventS2C : INetSerializable, IComparable<KOProjectHitPlayerNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ProjectileId;
        public ushort HitPlayerId;
        public Vector2 HitPosition;

        public KOProjectHitPlayerNetEventS2C(int occuredOnTick, ushort projectileId, ushort hitPlayerId, Vector2 hitPosition)
        {
            OccuredOnTick = occuredOnTick;
            ProjectileId = projectileId;
            HitPlayerId = hitPlayerId;
            HitPosition = hitPosition;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(ProjectileId);
            writer.Put((byte)HitPlayerId);
            writer.PutVector2Quantized(HitPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            HitPlayerId = reader.GetByte();
            HitPosition = reader.GetVector2Quantized();
        }

        public int CompareTo(KOProjectHitPlayerNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
