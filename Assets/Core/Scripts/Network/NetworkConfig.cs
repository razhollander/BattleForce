using UnityEngine;

namespace Core.Scripts.Network
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "BF/Network/Network Config")]
    public class NetworkConfig : ScriptableObject
    {
        public int MaxConnectedPlayers = 8;
        public int MaxConcurrentBullets = 256;
        public int TicksPerSeconds = 60;
        public float DeltaTime = 1/60f;
        public int ServerTicksBuffer = 2;
        public int Port = 7777;
        public string IpAddress = "localhost";
        public string ConntectionKey = "BattleForceGame";
    }
}