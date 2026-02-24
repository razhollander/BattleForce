using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentSpringConfig", menuName = "BF/Shared/Environment Springs Config")]
    public class EnvironmentSpringsConfig : ScriptableObject
    {
        public float Force = 20f;
        public float MaxSpin = 55f;
        public float MinSpin = 50f;
        public Vector2 Size = new Vector2(0.5f, 1f);
    }
}
