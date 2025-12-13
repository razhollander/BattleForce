using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents
{
    public struct PlayerJoinAcceptPacketS2C : INetSerializable
    {
        public int OccuredOnTick;
        public int NetPeerId;
        public ushort PlayerId;
        public string PlayerName;
        public PlayerSpaceshipStateS2C SpaceshipState;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(NetPeerId);
            writer.Put((byte)PlayerId);
            writer.Put(PlayerName);
            SpaceshipState.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            NetPeerId = reader.GetInt();
            PlayerId = reader.GetByte();
            PlayerName = reader.GetString();
            SpaceshipState.Deserialize(reader);
        }
    }
}