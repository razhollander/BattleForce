using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class WaterGunTalentConfig
    {
        public float DurationInSeconds = 3f;
        public float ConeAngleDegrees = 45f;
        public float ConeRange = 15f;
        public float EnemyPushForcePerTick = 40f;
        public float CasterRecoilForcePerTick = 15f;
    }
}
