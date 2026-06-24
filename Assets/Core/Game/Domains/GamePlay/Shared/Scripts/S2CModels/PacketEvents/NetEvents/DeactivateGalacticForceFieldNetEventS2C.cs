using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateGalacticForceFieldNetEventS2C : INetSerializable, IComparable<DeactivateGalacticForceFieldNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort FieldId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(FieldId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            FieldId = reader.GetUShort();
        }

        public int CompareTo(DeactivateGalacticForceFieldNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
