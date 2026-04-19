using System;
using LiteNetLib.Utils;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct ChickenEggHitNetEventS2C : INetSerializable, IComparable<ChickenEggHitNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort EggId;
        public ushort HitPlayerId;
        public Vector2 Position;

        public ChickenEggHitNetEventS2C(int occuredOnTick, ushort eggId, ushort hitPlayerId, Vector2 position)
        {
            OccuredOnTick = occuredOnTick;
            EggId = eggId;
            HitPlayerId = hitPlayerId;
            Position = position;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(EggId);
            writer.Put((byte)HitPlayerId);
            writer.PutVector2Quantized(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            EggId = reader.GetUShort();
            HitPlayerId = reader.GetByte();
            Position = reader.GetVector2Quantized();
        }

        public int CompareTo(ChickenEggHitNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
