using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct GoldenMoleDamagedNetEventS2C : INetSerializable, IComparable<GoldenMoleDamagedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort MoleId;
        public byte RemainingLives;
        public byte MaxLives;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)MoleId);
            writer.Put(RemainingLives);
            writer.Put(MaxLives);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            MoleId = reader.GetByte();
            RemainingLives = reader.GetByte();
            MaxLives = reader.GetByte();
        }

        public int CompareTo(GoldenMoleDamagedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
