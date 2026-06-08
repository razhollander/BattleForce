using Core.Game.Domains.GamePlay.Shared.Scripts.C2SModels;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public class JoinRequestPacketC2S : INetSerializable
    {
        public long ClientId;
        public FixedUnorderedList<PlayerJoinedDataC2S> PlayerJoinedList;
        
        public JoinRequestPacketC2S(int maxPlayers)
        {
            PlayerJoinedList = new FixedUnorderedList<PlayerJoinedDataC2S>(maxPlayers);
        }

        public void AddPlayer(string playerName, bool isGamepad, int inputDeviceId)
        {
            ref var playerJoined = ref PlayerJoinedList.AddAndGet();
            playerJoined.PlayerName = playerName;
            playerJoined.IsGamepad = isGamepad;
            playerJoined.InputDeviceId = inputDeviceId;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ClientId);
            writer.Put((byte)PlayerJoinedList.Count);

            foreach (var playerJoined in PlayerJoinedList.AsSpan())
            {
                writer.Put(playerJoined.PlayerName);
                writer.Put(playerJoined.IsGamepad);
                writer.Put(playerJoined.InputDeviceId);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            ClientId = reader.GetLong();
            byte playersCount = reader.GetByte();
            PlayerJoinedList.Clear();

            for (int i = 0; i < playersCount; i++)
            {
                ref var playerJoined = ref PlayerJoinedList.AddAndGet();
                playerJoined.PlayerName = reader.GetString();
                playerJoined.IsGamepad = reader.GetBool();
                playerJoined.InputDeviceId = reader.GetInt();
            }
        }
    }
}