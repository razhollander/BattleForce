using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public class JoinRequestPacketC2S : INetSerializable
    {
        public JoinRequestPacketC2S(string playerName)
        {
            PlayerName = playerName;
        }

        public JoinRequestPacketC2S()
        {
        }

        public string PlayerName { get; set; }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerName);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerName = reader.GetString();
        }
    }
}