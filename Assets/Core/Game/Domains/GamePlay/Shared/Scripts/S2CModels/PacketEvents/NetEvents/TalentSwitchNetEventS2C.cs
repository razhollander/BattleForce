using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct TalentSwitchNetEventS2C : INetSerializable, IComparable<TalentSwitchNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public int NewTalentIndex;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PlayerId);
            writer.Put(NewTalentIndex);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetUShort();
            NewTalentIndex = reader.GetInt();
        }

        public int CompareTo(TalentSwitchNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}