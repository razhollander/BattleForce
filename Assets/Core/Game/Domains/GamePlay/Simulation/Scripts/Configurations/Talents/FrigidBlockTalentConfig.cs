using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class FrigidBlockTalentConfig
    {
        public float ProjectileSpeed = 30f;
        public float SpawnGapFromCaster = 0.5f;
        public float LinearDamping = 1.2f;
        public float AngularDamping = 1.5f;
        public float Density = 25f; // high so players barely push the block (reads as a heavy moving wall)
        public float Restitution = 0.1f;
        public float IdleLinearVelocityThreshold = 0.15f;
        public float IdleAngularVelocityThreshold = 0.15f;
        public float SecondsIdleUntilDestroy = 3f;
    }
}
