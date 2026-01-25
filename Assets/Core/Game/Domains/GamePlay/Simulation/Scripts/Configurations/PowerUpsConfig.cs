using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "PowerUpsNetworkConfig", menuName = "BF/Network/Power Ups Config")]
    public class PowerUpsConfig : ScriptableObject
    {
        public float SpawnMinSecondsInterval = 5f;
        public float SpawnMaxSecondsInterval = 10f;
        public int MaxConcurrentPowerUpBalls = 5;
        public float MoveSpeed = 5f;
        public float Radius = 0.5f;
    }
}
