using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerBulletS2C : INetSerializable
    {
        public ushort Id;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Direction;
        public float Radius;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)BelongToPlayerId);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(Radius);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            BelongToPlayerId = reader.GetByte();
            Position = reader.GetVector2Quantized();
            Radius = reader.GetFloat16();
        }

        public void SerializeTransforms(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.PutVector2Quantized(Position);
        }

        public void DeserializeTransforms(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2Quantized();
        }
    }
}