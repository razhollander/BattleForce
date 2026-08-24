namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class GatePassConfig
    {
        public float StageDurationSeconds = 60f;
        public ushort ScorePerPass = 1;
        // Consecutive passes by the same team through a gate multiply the score (x1, x2, ...) up to this cap. A pass by a
        // different team resets that gate's streak back to x1.
        public int MaxGatePassMultiplier = 4;
        public float PassScoreCooldownSeconds = 0.4f; // one player cannot re-score on the same gate faster than this
        // A single-tick movement longer than this (in unscaled units) is a teleport (Swap, Soul respawn, teleport gate),
        // not real travel, so it is never counted as a gate pass. Real per-tick movement is a fraction of a unit.
        public float TeleportDetectionSegmentLength = 8f;

        // ScoreGateObstacle mass + solver tuning. Only the server body reads these (the client view reads gate geometry
        // from SharedGamePlayConfig). Direct control over how heavy the gate feels: when ScoreGateMass > 0 it overrides
        // the density-derived mass, so tuning that one number changes how far a ram/talent shoves the gate. When 0, the
        // mass falls back to ScoreGateDensity * area.
        public float ScoreGateMass = 2f;
        public float ScoreGateDensity = 2f; // used to build the fixtures; ScoreGateMass overrides the resulting mass when > 0
        public float ScoreGateRestitution = 0.2f;
        public float ScoreGateLinearDamping = 1.5f; // a shoved gate drifts and settles instead of sliding forever
        public float ScoreGateAngularDamping = 1.5f; // a spun gate decays after a couple of turns

        // How hard each talent shoves the gate. Values are impulse PER UNIT MASS (and spin PER UNIT INERTIA), so they
        // stay meaningful when ScoreGateDensity changes. Ram/Rock/FrigidBlock/bullets push through the solver and need
        // no value here; these cover the talents whose projectiles are sensors (no solver impulse) or that hit via a cast.
        //
        // Because the impulse is per unit mass, each value below IS the gate velocity it produces, which makes the whole
        // block directly comparable against a plain ram. A ram is resolved by the solver instead, at
        // (1 + ScoreGateRestitution) * ramSpeed * playerMass / (playerMass + ScoreGateMass), so raising ScoreGateMass
        // weakens every ram while leaving these untouched - move the mass and this block has to move with it, or talents
        // and rams drift apart.
        public float KOPushImpulse = 72f;
        public float KOSpinImpulse = 36f;
        public float HeadbuttPushImpulse = 42f;
        public float HeadbuttSpinImpulse = 12f;
        public float ChickenEggSpinImpulse = 4f; // spin only, no push - a small static egg twists the gate a little
        public float GrapplingHookReactionImpulse = 24f; // small push toward the caster as the hook anchors
        public float MagneticPullImpulse = 72f; // pulls the gate toward the caster
        public float YearsOfPainPushImpulse = 96f;
        public float YearsOfPainSpinImpulse = 36f;
        public float WaterGunPushImpulsePerSecond = 120f; // applied continuously, scaled by delta time
        // The nuke launches every gate on the map at once off a power-up nobody can rely on, so it sits above the
        // strongest talent - nothing on a cooldown should throw a gate further.
        public float NukePushImpulse = 120f;
        public float NukeSpinImpulse = 45f; // random direction per gate
    }
}
