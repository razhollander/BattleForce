using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentFishingRodProjectileStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort PlayerCasterId;
        public Vector2 Position;
        public Vector2 Velocity;
        public FishingRodTipPhase Phase;
        public ushort CaughtEnemyId;
        public FishingRodCaughtEnemyType CaughtEnemyType; // whether CaughtEnemyId is a player id or a mole id
        public Vector2 EnemyCaughtArrowDirection;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put((byte)PlayerCasterId);
            writer.PutVector2Quantized(Position);
            writer.Put((byte)Phase);
            writer.Put((byte)CaughtEnemyId);
            writer.Put((byte)CaughtEnemyType);
            writer.PutVector2Quantized(EnemyCaughtArrowDirection);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            PlayerCasterId = reader.GetByte();
            Position = reader.GetVector2Quantized();
            Phase = (FishingRodTipPhase)reader.GetByte();
            CaughtEnemyId = reader.GetByte();
            CaughtEnemyType = (FishingRodCaughtEnemyType)reader.GetByte();
            EnemyCaughtArrowDirection = reader.GetVector2Quantized();
        }

        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
            writer.PutVector2Quantized(EnemyCaughtArrowDirection);
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            EnemyCaughtArrowDirection = reader.GetVector2Quantized();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
