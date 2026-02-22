using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayersSwapNetEventS2C : INetSerializable, IComparable<PlayersSwapNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public ushort OtherPlayerId;
        public Vector2 CasterPosition; // check maybe dont need all of these because the client knows them
        public Vector2 OtherPosition;
        public Vector2 CasterDirection;
        public Vector2 OtherDirection;

        public PlayersSwapNetEventS2C(int occuredOnTick, ushort casterPlayerId, ushort otherPlayerId, Vector2 casterPosition, Vector2 otherPosition, Vector2 casterDirection, Vector2 otherDirection)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            OtherPlayerId = otherPlayerId;
            CasterPosition = casterPosition;
            OtherPosition = otherPosition;
            CasterDirection = casterDirection;
            OtherDirection = otherDirection;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)OtherPlayerId);
            writer.PutVector2Quantized(CasterPosition);
            writer.PutVector2Quantized(OtherPosition);
            writer.PutVector2AsAngle16(CasterDirection);
            writer.PutVector2AsAngle16(OtherDirection);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            OtherPlayerId = reader.GetByte();
            CasterPosition = reader.GetVector2Quantized();
            OtherPosition = reader.GetVector2Quantized();
            CasterDirection = reader.GetVector2FromAngle16();
            OtherDirection = reader.GetVector2FromAngle16();
        }

        public int CompareTo(PlayersSwapNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}