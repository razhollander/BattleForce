using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class RockTalentConfig
    {
        public float DurationInSeconds = 4f;
        public float ColliderRadiusMultiplier = 2f;
        public float BodyDensity = 1000f; // Huge mass so static (rotating) walls still push the rock out fully while dynamic bodies (players, FrigidBlock) bounce off without moving it.
        public float Restitution = 1f;
        public float EnemyPushForce = 20f;
        public float EnemySpinAmount = 50f;
    }
}
