using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [CreateAssetMenu(fileName = "KOTalentConfig", menuName = "BF/Network/Talents/KO Talent Config")]
    public class KOTalentConfig : ScriptableObject
    {
        public float ProjectileSpeed = 20f;
        public float ProjectileSize = 1f;
        public float ReturnSpeedMultiplier = 2f;
        public float MaxFirstPhaseDuration = 2f;
        public float PushForce = 50f;
        public float MaxSpin = 55f;
        public float MinSpin = 50f;
    }
}
