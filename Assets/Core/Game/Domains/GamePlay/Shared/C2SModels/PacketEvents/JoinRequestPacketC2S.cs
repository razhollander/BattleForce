using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.ServerToClientModels
{
    public class JoinRequestPacketC2S: INetSerializable
    {
        public string UserName { get; set; }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserName);
        }

        public void Deserialize(NetDataReader reader)
        {
            UserName = reader.GetString();
        }
    }
}