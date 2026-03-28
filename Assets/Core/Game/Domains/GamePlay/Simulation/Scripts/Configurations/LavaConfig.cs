using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "LavaConfig", menuName = "BF/Simulation/Lava Config")]
    public class LavaConfig : ScriptableObject
    {
        public ushort DamageAmount = 1;
        public float DamageIntervalInSeconds = 1.0f;
    }
}
