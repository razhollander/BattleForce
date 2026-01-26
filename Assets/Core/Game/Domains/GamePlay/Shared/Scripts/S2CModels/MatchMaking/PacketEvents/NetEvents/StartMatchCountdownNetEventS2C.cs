using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents
{
    public struct StartMatchCountdownNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public float Seconds;

        public StartMatchCountdownNetEventS2C(int occuredOnTick, float seconds)
        {
            OccuredOnTick = occuredOnTick;
            Seconds = seconds;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(Seconds);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            Seconds = reader.GetFloat();
        }
    }
}
