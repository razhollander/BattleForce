using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations.Talents
{
    [Serializable]
    public class HammerTalentConfig
    {
        public float CooldownInSeconds = 10f;
        public float HammerSpeed = 10f;
        public float HammerSize = 1;
        public float HammerMass = 10;
    }
}