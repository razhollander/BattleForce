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
        public int PhysicsVelocityIterations = 8;
        public int PositionIterations = 8;
        public int ServerTicksBuffer = 2;
        public int HostPort = 49153;
        public string IpAddress = "localhost"; // 109.67.156.134
        public string ConntectionKey = "BattleForceGame";
    }
}