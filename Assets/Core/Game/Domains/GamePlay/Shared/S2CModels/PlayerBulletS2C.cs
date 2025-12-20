using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerBulletS2C : INetSerializable
    {
        public int Id;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public float MoveSpeed;
        public Vector2 Direction;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)BelongToPlayerId);
            writer.Put(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            BelongToPlayerId = reader.GetByte();
            Position = reader.GetVector2();
        }

        public void SerializeTransforms(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(Position);
        }

        public void DeserializeTransforms(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2();
        }
    }
}