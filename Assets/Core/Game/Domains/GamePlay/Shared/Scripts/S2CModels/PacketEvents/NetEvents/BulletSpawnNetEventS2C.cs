using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct BulletSpawnNetEventS2C : INetSerializable, IComparable<BulletSpawnNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort BulletId;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public float BulletRadius;
        public Vector2 Velocity;

        public BulletSpawnNetEventS2C(int occuredOnTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius, Vector2 velocity)
        {
            OccuredOnTick = occuredOnTick;
            BulletId = bulletId;
            BelongToPlayerId = belongToPlayerId;
            Position = position;
            BulletRadius = bulletRadius;
            Velocity = velocity;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(BulletId);
            writer.Put((byte)BelongToPlayerId);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(BulletRadius);
            writer.PutVector2Quantized(Velocity);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            BulletId = reader.GetUShort();
            BelongToPlayerId = reader.GetByte();
            Position = reader.GetVector2Quantized();
            BulletRadius = reader.GetFloat16();
            Velocity = reader.GetVector2Quantized();
        }
        
        public int CompareTo(BulletSpawnNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}