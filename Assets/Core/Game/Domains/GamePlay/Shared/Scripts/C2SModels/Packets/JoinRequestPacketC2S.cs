using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public class JoinRequestPacketC2S : INetSerializable
    {
        public string PlayerName { get; private set; }
        public bool IsGamePadEnabled { get; private set; }

        public JoinRequestPacketC2S(string playerName, bool isGamePadEnabled)
        {
            PlayerName = playerName;
            IsGamePadEnabled = isGamePadEnabled;
        }

        public JoinRequestPacketC2S()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerName);
            writer.Put(IsGamePadEnabled);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerName = reader.GetString();
            IsGamePadEnabled = reader.GetBool();
        }
    }
}