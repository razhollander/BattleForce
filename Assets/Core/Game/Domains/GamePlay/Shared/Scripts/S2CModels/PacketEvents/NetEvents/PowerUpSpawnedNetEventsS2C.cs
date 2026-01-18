using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct PowerUpSpawnedNetEventsS2C : INetSerializable, IComparable<PowerUpSpawnedNetEventsS2C>
    {
        public int OccuredOnTick;
        public ushort Id;
        public PowerUpType Type;
        public Vector2 Position;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(Id);
            writer.Put((byte)Type);
            writer.Put(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            Id = reader.GetUShort();
            Type = (PowerUpType)reader.GetByte();
            Position = reader.GetVector2();
        }

        public int CompareTo(PowerUpSpawnedNetEventsS2C other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}
