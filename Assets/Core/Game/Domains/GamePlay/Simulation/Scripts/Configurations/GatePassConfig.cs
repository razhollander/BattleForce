namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class GatePassConfig
    {
        public float StageDurationSeconds = 60f;
        public int ScorePerPass = 1;
        public float PassScoreCooldownSeconds = 0.4f; // one player cannot re-score on the same gate faster than this
        // A single-tick movement longer than this (in unscaled units) is a teleport (Swap, Soul respawn, teleport gate),
        // not real travel, so it is never counted as a gate pass. Real per-tick movement is a fraction of a unit.
        public float TeleportDetectionSegmentLength = 8f;

        // How hard each talent shoves the gate. Values are impulse PER UNIT MASS (and spin PER UNIT INERTIA), so they
        // stay meaningful when ScoreGateDensity changes. Ram/Rock/FrigidBlock/bullets push through the solver and need
        // no value here; these cover the talents whose projectiles are sensors (no solver impulse) or that hit via a cast.
        public float KOPushImpulse = 6f;
        public float KOSpinImpulse = 3f;
        public float HeadbuttPushImpulse = 14f;
        public float HeadbuttSpinImpulse = 4f;
        public float ChickenEggSpinImpulse = 5f; // spin only, no push - a small static egg twists the gate a little
        public float GrapplingHookReactionImpulse = 2f; // small push toward the caster as the hook anchors
        public float MagneticPullImpulse = 6f; // pulls the gate toward the caster
        public float YearsOfPainPushImpulse = 8f;
        public float YearsOfPainSpinImpulse = 3f;
        public float WaterGunPushImpulsePerSecond = 10f; // applied continuously, scaled by delta time
    }
}
