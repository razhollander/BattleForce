using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.ServerToClientModels
{
    public class JoinAcceptPacketS2C : INetSerializable
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PlayerId);
            writer.Put(PlayerName);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = (int)reader.GetByte();
            PlayerName = reader.GetString();
        }
    }
}