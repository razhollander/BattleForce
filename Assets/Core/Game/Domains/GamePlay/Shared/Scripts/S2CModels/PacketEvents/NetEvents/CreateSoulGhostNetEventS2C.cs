using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateSoulGhostNetEventS2C : INetSerializable, IComparable<CreateSoulGhostNetEventS2C>
    {
        public int OccuredOnTick;
        public TalentSoulGhostStateS2C SoulGhost;

        public CreateSoulGhostNetEventS2C(int occuredOnTick, TalentSoulGhostStateS2C soulGhost)
        {
            OccuredOnTick = occuredOnTick;
            SoulGhost = soulGhost;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(SoulGhost);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            SoulGhost.Deserialize(reader);
        }

        public int CompareTo(CreateSoulGhostNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
