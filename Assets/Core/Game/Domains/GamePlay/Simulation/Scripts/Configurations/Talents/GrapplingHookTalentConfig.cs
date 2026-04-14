using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class GrapplingHookTalentConfig
    {
        public float ProjectileSpeed = 25f;
        public float MaxDistance = 30f;
        public float ArriveDistance = 2f;
        public float PullForce = 1f;
        public float PlayerVelocitySquaredThresholdToDeactivateHook = 0.1f;
        public float GraceTicksUntilCheckIfVelocityIsBelowThreshold = 5f;
    }
}
