using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerSetMoveDestinationPointNetEventS2C : INetSerializable, IComparable<PlayerSetMoveDestinationPointNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public Vector2 DestinationPoint;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.PutVector2Quantized(DestinationPoint);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            DestinationPoint = reader.GetVector2Quantized();
        }

        public int CompareTo(PlayerSetMoveDestinationPointNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
