using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct BulletDestroyedNetEventS2C : INetSerializable, IComparable<BulletDestroyedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort BulletId;
        public Vector2 Position;

        public BulletDestroyedNetEventS2C(int occuredOnTick, ushort bulletId, Vector2 position)
        {
            OccuredOnTick = occuredOnTick;
            BulletId = bulletId;
            Position = position;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(BulletId);
            writer.PutVector2Quantized(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            BulletId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
        }

        public int CompareTo(BulletDestroyedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}