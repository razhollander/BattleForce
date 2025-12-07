using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents
{
    public class JoinAcceptPacketS2C : INetSerializable
    {
        public int TickOnServer;
        public int PlayerId;
        public string PlayerName;
        public PlayerSpaceshipStateS2C SpaceshipState;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(TickOnServer);
            writer.Put((byte)PlayerId);
            writer.Put(PlayerName);
            SpaceshipState.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            TickOnServer = reader.GetInt();
            PlayerId = reader.GetByte();
            PlayerName = reader.GetString();
            SpaceshipState.Deserialize(reader);
        }
    }
}