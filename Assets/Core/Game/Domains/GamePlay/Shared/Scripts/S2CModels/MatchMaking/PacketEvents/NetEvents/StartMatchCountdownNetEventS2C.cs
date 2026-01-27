using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents
{
    public struct StartMatchCountdownNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort CountdownSeconds;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CountdownSeconds);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CountdownSeconds = reader.GetByte();
        }
    }
}
