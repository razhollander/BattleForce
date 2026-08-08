using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct FishingRodThrowNetEventS2C : INetSerializable, IComparable<FishingRodThrowNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public ushort ThrownEnemyId;
        public FishingRodCaughtEnemyType ThrownEnemyType; // whether ThrownEnemyId is a player id or a mole id - a mole is spun in place instead of being thrown
        public Vector2 ThrowDirection;

        public FishingRodThrowNetEventS2C(int occuredOnTick, ushort casterPlayerId, ushort thrownEnemyId, FishingRodCaughtEnemyType thrownEnemyType, Vector2 throwDirection)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            ThrownEnemyId = thrownEnemyId;
            ThrownEnemyType = thrownEnemyType;
            ThrowDirection = throwDirection;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)ThrownEnemyId);
            writer.Put((byte)ThrownEnemyType);
            writer.PutVector2AsAngle16(ThrowDirection);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            ThrownEnemyId = reader.GetByte();
            ThrownEnemyType = (FishingRodCaughtEnemyType)reader.GetByte();
            ThrowDirection = reader.GetVector2FromAngle16();
        }

        public int CompareTo(FishingRodThrowNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
