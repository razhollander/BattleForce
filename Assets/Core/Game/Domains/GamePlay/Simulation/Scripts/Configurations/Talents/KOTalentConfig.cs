using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class KOTalentConfig
    {
        public float ProjectileSpeed = 50f;
        public float ProjectileSize = 1f;
        public float ReturnSpeedMultiplier = 2f;
        public float MaxFirstPhaseDuration = 0.3f;
        public float PushForce = 50f;
        public float MaxSpin = 55f;
        public float MinSpin = 50f;
    }
}
