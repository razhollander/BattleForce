using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class MagneticPullTalentConfig
    {
        public float FieldArcAngle = 90f; 
        public float PushForce = 30f;
        public float MaxSpin = 55f;
        public float MinSpin = 50f;
    }
}
