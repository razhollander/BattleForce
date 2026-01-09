using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct BulletSpawnNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort BulletId;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public float BulletRadius;

        public BulletSpawnNetEventS2C(int occuredOnTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius)
        {
            OccuredOnTick = occuredOnTick;
            BulletId = bulletId;
            BelongToPlayerId = belongToPlayerId;
            Position = position;
            BulletRadius = bulletRadius;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(BulletId);
            writer.Put((byte)BelongToPlayerId);
            writer.Put(Position);
            writer.Put(BulletRadius);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            BulletId = reader.GetUShort();
            BelongToPlayerId = reader.GetByte();
            Position = reader.GetVector2();
            BulletRadius = reader.GetFloat();
        }
    }
}