using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class ChickenTalentConfig
    {
        public float CountdownDuration = 3f;
        public float PushForce = 10f;
        public float DestroyDelayInSeconds = 5f;
        public float SpinAmount = 5f;
    }
}
