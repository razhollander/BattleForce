using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public class JoinRequestPacketC2S : INetSerializable
    {
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