using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct MoleExpiredNetEventS2C : INetSerializable, IComparable<MoleExpiredNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort MoleId;
        public ushort MoleHoleId;
        public int HideOnTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)MoleId);
            writer.Put((byte)MoleHoleId);
            writer.Put(HideOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            MoleId = reader.GetByte();
            MoleHoleId = reader.GetByte();
            HideOnTick = reader.GetInt();
        }

        public int CompareTo(MoleExpiredNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
