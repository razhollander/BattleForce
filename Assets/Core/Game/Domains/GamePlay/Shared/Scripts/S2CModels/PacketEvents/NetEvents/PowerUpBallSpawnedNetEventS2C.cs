using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct PowerUpBallSpawnedNetEventS2C : INetSerializable, IComparable<PowerUpBallSpawnedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PowerUpBallId;
        public Vector2 Position;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PowerUpBallId);
            writer.PutVector2Quantized(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PowerUpBallId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
        }

        public int CompareTo(PowerUpBallSpawnedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}