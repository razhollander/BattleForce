using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct BulletSpawnNetEventS2C : INetSerializable, IComparable<BulletSpawnNetEventS2C>
    {
        public ushort SequenceId;
        public ushort BulletId;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        
        public BulletSpawnNetEventS2C(ushort sequenceId, ushort bulletId, ushort belongToPlayerId, Vector2 position)
        {
            SequenceId = sequenceId;
            BulletId = bulletId;
            BelongToPlayerId = belongToPlayerId;
            Position = position;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SequenceId);
            writer.Put(BulletId);
            writer.Put(BelongToPlayerId);
            writer.Put(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            SequenceId = reader.GetUShort();
            BulletId = reader.GetUShort();
            BelongToPlayerId = reader.GetUShort();
            Position = reader.GetVector2();
        }

        public int CompareTo(BulletSpawnNetEventS2C other)
        {
            return SequenceId.CompareTo(other.SequenceId);
        }
    }
}