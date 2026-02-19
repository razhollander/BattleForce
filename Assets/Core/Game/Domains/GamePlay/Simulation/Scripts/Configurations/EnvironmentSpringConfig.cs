using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "EnvironmentSpringConfig", menuName = "BF/Simulation/Environment Spring Config")]
    public class EnvironmentSpringConfig : ScriptableObject
    {
        public float Force = 20f;
        public float Spin = 20f;
        public Vector2 Size = new Vector2(1f, 1f);
    }
}
