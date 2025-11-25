using UnityEngine;

namespace Core.Scripts.Network
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "BF/Network/Network Config")]
    public class NetworkConfig : ScriptableObject
    {
        public int MaxConnectedPlayers = 8;
        public int TicksPerSeconds = 60;
        public int Port = 7777;
        public string IpAddress = "localhost";
        public string ConntectionKey = "BattleForceGame";
    }
}