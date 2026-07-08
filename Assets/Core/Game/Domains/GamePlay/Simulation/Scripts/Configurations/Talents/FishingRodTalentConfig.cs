using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class FishingRodTalentConfig
    {
        public float TipSpeed = 45f;
        public float ReturnSpeedMultiplier = 2f;
        public float ArriveDistance = 2f;
        public float TipMaxLifetimeSeconds = 1.5f;
        public float ThrowWindowSeconds = 3f;
        public float ThrowPushForce = 250f;
        public float ThrowMinSpin = 20f;
        public float ThrowMaxSpin = 35f;
    }
}
