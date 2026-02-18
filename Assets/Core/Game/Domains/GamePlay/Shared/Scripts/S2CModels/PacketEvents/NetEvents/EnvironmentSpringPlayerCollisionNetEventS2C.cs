using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct EnvironmentSpringPlayerCollisionNetEventS2C : INetSerializable, IComparable<EnvironmentSpringPlayerCollisionNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort SpringId;
        public ushort PlayerId;
        public Vector2 NewPlayerDirection;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(SpringId);
            writer.Put(PlayerId);
            writer.PutVector2Quantized(NewPlayerDirection);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            SpringId = reader.GetUShort();
            PlayerId = reader.GetUShort();
            NewPlayerDirection = reader.GetVector2Quantized();
        }

        public int CompareTo(EnvironmentSpringPlayerCollisionNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
