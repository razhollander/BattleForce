using System;
using LiteNetLib.Utils;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct LayChickenEggNetEventS2C : INetSerializable, IComparable<LayChickenEggNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public ushort EggId;
        public Vector2 Position;

        public LayChickenEggNetEventS2C(int occuredOnTick, ushort casterPlayerId, ushort eggId, Vector2 position)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            EggId = eggId;
            Position = position;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put(EggId);
            writer.PutVector2Quantized(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            EggId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
        }

        public int CompareTo(LayChickenEggNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
