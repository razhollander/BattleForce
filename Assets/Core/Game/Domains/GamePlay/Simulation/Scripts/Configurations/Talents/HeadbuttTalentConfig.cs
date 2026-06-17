using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class HeadbuttTalentConfig
    {
        public float MaxChargeForce = 30f;
        public float MaxChargeDurationSeconds = 2f;
        public float MaxDashWindowSeconds = 1.5f;
        public float MinDashWindowSeconds = 0.3f;
        public float EnemyPushForce = 25f;
        public float EnemySpinAmount = 15f;
        public float CasterVelocityDamping = 0.05f;
    }
}
