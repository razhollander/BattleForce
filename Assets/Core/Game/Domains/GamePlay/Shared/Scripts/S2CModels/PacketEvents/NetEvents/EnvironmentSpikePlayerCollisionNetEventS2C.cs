using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct EnvironmentSpikePlayerCollisionNetEventS2C : INetSerializable, IComparable<EnvironmentSpikePlayerCollisionNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort SpikeId;
        public ushort PlayerId;
        public Vector2 NewPlayerDirection;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)SpikeId);
            writer.Put((byte)PlayerId);
            writer.PutVector2Quantized(NewPlayerDirection);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            SpikeId = reader.GetByte();
            PlayerId = reader.GetByte();
            NewPlayerDirection = reader.GetVector2Quantized();
        }

        public int CompareTo(EnvironmentSpikePlayerCollisionNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
