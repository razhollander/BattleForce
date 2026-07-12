using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class FrozenTalentConfig
    {
        // How long the caster stays frozen before it auto-deactivates (also cancellable with a second talent press).
        public float DurationInSeconds = 4f;
    }
}
