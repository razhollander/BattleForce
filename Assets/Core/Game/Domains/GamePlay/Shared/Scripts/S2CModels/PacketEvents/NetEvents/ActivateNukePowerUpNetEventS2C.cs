using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct ActivateNukePowerUpNetEventS2C : INetSerializable, IComparable<ActivateNukePowerUpNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public Vector2 CasterPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.PutVector2Quantized(CasterPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            CasterPosition = reader.GetVector2Quantized();
        }

        public int CompareTo(ActivateNukePowerUpNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
