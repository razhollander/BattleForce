using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [CreateAssetMenu(fileName = "KOTalentConfig", menuName = "BF/Network/Talents/KO Talent Config")]
    public class KOTalentConfig : ScriptableObject
    {
        public float ProjectileSpeed = 20f;
        public float ProjectileSize = 1f;
        public float ReturnSpeedMultiplier = 2f;
        public float MaxDuration = 2f;
        public float PushForce = 50f;
        public float SpinForce = 500f;
        public float DurationEngineOffSeconds = 2f;
    }
}
