using System.Numerics;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct TalentCardObtainedNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort TalentCardId;
        public ushort ObtainedByPlayerId;

        public TalentCardObtainedNetEventS2C(int occuredOnTick, ushort talentCardId, ushort obtainedByPlayerId)
        {
            OccuredOnTick = occuredOnTick;
            TalentCardId = talentCardId;
            ObtainedByPlayerId = obtainedByPlayerId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(TalentCardId);
            writer.Put(ObtainedByPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            TalentCardId = reader.GetUShort();
            ObtainedByPlayerId = reader.GetUShort();
        }
    }
}