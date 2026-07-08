using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class RockTalentConfig
    {
        public float DurationInSeconds = 4f;
        public float ColliderRadiusMultiplier = 2f;
        // Huge mass so static (rotating) walls still push the rock out fully while dynamic bodies (players, FrigidBlock) bounce off without moving it.
        public float BodyDensity = 1000f;
        public float Restitution = 1f;
        // On entering rock state, enemies within this radius are shoved and spun away.
        public float EnemyPushRadius = 4f;
        public float EnemyPushForce = 20f;
        public float EnemySpinAmount = 50f;
    }
}
