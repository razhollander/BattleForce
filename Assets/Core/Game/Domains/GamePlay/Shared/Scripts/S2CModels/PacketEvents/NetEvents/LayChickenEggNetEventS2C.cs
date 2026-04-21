using System;
using System.Numerics;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct LayChickenEggNetEventS2C : INetSerializable, IComparable<LayChickenEggNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public Vector2 Position;
        public ushort EggId;

        public LayChickenEggNetEventS2C(int occuredOnTick, ushort casterPlayerId, Vector2 position, ushort eggId)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            Position = position;
            EggId = eggId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put(EggId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            EggId = reader.GetUShort();
        }

        public int CompareTo(LayChickenEggNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
