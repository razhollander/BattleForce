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
        public Vector2 StartPosition;
        public Vector2 Position;
        public Vector2 Velocity;
        public FishingRodTipPhase Phase;
        public ushort CaughtEnemyId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(PlayerCasterId);
            writer.PutVector2Quantized(Position);
            writer.Put((byte)Phase);
            writer.Put(CaughtEnemyId);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            PlayerCasterId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            Phase = (FishingRodTipPhase)reader.GetByte();
            CaughtEnemyId = reader.GetUShort();
        }

        public void SerializeDelta(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(Position);
        }

        public void DeserializeDelta(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2Quantized();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
