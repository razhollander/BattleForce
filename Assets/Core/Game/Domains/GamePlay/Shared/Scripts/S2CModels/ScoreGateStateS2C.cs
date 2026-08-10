using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct ScoreGateStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public Vector2 Position;
        public Vector2 Rotation; // unit facing vector; the gap axis is perpendicular to it
        public ushort LastScoredTeamId; // 0 = never scored; drives the gate tint, survives rejoin

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(Rotation.X);
            writer.PutFloat16(Rotation.Y);
            writer.Put((byte)LastScoredTeamId);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2Quantized();
            var rotationX = reader.GetFloat16();
            var rotationY = reader.GetFloat16();
            Rotation = new Vector2(rotationX, rotationY);
            LastScoredTeamId = reader.GetByte();
        }

        // Per-tick transform update (the gate is physics-driven, so only its moving transform changes each tick).
        // LastScoredTeamId is not sent here - the tint is driven by the ScoreGatePassed net event and the rejoin snapshot.
        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.PutVector2Quantized(Position);
            writer.PutFloat16(Rotation.X);
            writer.PutFloat16(Rotation.Y);
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2Quantized();
            var rotationX = reader.GetFloat16();
            var rotationY = reader.GetFloat16();
            Rotation = new Vector2(rotationX, rotationY);
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
