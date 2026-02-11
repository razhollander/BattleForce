using LiteNetLib.Utils;
using System;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct StartMatchEligibleChangedNetEventS2C : INetSerializable, IComparable<StartMatchEligibleChangedNetEventS2C>
    {
        public bool IsEligible;
        public int OccuredOnTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsEligible);
            writer.Put(OccuredOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            IsEligible = reader.GetBool();
            OccuredOnTick = reader.GetInt();
        }

        public int CompareTo(StartMatchEligibleChangedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
