using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents
{
    public class JoinAcceptPacketS2C : INetSerializable
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public PlayerTransformStateS2C PlayerTransform { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PlayerId);
            writer.Put(PlayerName);
            PlayerTransform.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetByte();
            PlayerName = reader.GetString();
            PlayerTransform.Deserialize(reader);
        }
    }
}