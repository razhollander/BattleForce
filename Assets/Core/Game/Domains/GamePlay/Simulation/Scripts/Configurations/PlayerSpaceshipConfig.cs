using ConditionalField;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class PlayerSpaceshipConfig
    {
        public ushort StartHealth = 5;
        public float TargetMovementSpeed = 5f;
        public float RotationSpeed = 150f;
        public float ShootCooldown = 0.35f;
        public float AutoShootRange = 12f;
        public float AutoShootAngleDegrees = 30f;
        public float DefaultPlayerRadius = 0.8f;
        public float EngineAcceleration = 70f;
        public float VelocityDecelerationPerSecond = 50f;
        public float SpinDecelerationPerSecond = 0.05f;
        public float MinSpin = 2f;
        public float MinVelocity = 0.01f;
        public float TurnEngineOnWhenReachVelocity = 0.25f;
        public float DefaultHeartRadius = 0.64f;
        public float LockOnTargetMaxRange = 13f;
        public float LockOnTargetHalfArcAngleDegrees = 45;
        public ushort LockOnTargetHitDamage = 1;
        public bool CanBarrelDash = true;
        public float BarrelDashForce = 40f;
        public float BarrelDashSpinAmount = 50f;
        [ConditionalField(nameof(CanBarrelDash), true)]
        public bool ShouldBarrelDashTowardsPlayerDirection = false;
    }
}