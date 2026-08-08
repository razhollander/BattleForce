using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct MoleExpiredNetEventS2C : INetSerializable, IComparable<MoleExpiredNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort MoleId;
        public int HideOnTick; // the tick the mole finishes its pre-hide shake and goes back into its hole, so the client shakes only until then

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)MoleId);
            writer.Put(HideOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            MoleId = reader.GetByte();
            HideOnTick = reader.GetInt();
        }

        public int CompareTo(MoleExpiredNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
