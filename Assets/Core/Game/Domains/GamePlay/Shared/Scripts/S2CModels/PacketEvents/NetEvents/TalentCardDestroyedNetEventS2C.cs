using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct TalentCardObtainedNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort CardId;

        public TalentCardObtainedNetEventS2C(int occuredOnTick, ushort cardId)
        {
            OccuredOnTick = occuredOnTick;
            CardId = cardId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(CardId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CardId = reader.GetUShort();
        }
    }
}