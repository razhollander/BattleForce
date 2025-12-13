using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct BulletSpawnNetEventS2C : INetSerializable
    {
        public ushort OccuredOnTick;
        public ushort BulletId;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        
        public BulletSpawnNetEventS2C(ushort occuredOnTick, ushort bulletId, ushort belongToPlayerId, Vector2 position)
        {
            OccuredOnTick = occuredOnTick;
            BulletId = bulletId;
            BelongToPlayerId = belongToPlayerId;
            Position = position;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(BulletId);
            writer.Put(BelongToPlayerId);
            writer.Put(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetUShort();
            BulletId = reader.GetUShort();
            BelongToPlayerId = reader.GetUShort();
            Position = reader.GetVector2();
        }
    }
}