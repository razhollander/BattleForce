using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents
{
    public struct StopMatchCountdownNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;

        public StopMatchCountdownNetEventS2C(int occuredOnTick)
        {
            OccuredOnTick = occuredOnTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
        }
    }
}
