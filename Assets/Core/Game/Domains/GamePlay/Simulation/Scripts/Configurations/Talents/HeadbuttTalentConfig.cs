using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class HeadbuttTalentConfig
    {
        public float MaxChargeForce = 30f;
        public float MaxSecondsInDash = 1.5f;
        public float MinSecondsInDash = 0.3f;
        public float EnemyPushForce = 25f;
        public float EnemySpinAmount = 15f;
    }
}
