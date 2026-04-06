using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class SentryGunTalentConfig
    {
        public float DurationInSeconds = 5f;
        public float ShootCooldownMultiplier = 0.5f;
    }
}
