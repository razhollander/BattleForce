using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents
{
    public struct StartMatchCountdownNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public float Duration;

        public StartMatchCountdownNetEventS2C(int occuredOnTick, float duration)
        {
            OccuredOnTick = occuredOnTick;
            Duration = duration;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(Duration);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            Duration = reader.GetFloat();
        }
    }
}
