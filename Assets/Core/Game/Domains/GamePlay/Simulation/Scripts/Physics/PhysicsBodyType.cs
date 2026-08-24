namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    [System.Flags]
    public enum PhysicsBodyType
    {
        None = 0,
        PlayerSpaceship = 1,
        Wall = 2,
        PlayerBullet = 3,
        TalentCard = 4,
        Lava = 5,
        PowerUpBall = 6,
        TeamFloor = 7,
        StartMatchWall = 8,
        EnvironmentSpring = 9,
        EnvironmentSpike = 17,
        EnvironmentTeleportGate = 10,
        SwapField = 11,
        KOProjectile = 12,
        GrapplingHookProjectile = 13,
        StageBoundary = 14,
        PlayerHeart = 15,
        ChickenEgg = 16,
        FrigidBlock = 18,
        FishingRodTip = 19,
        SoulGhost = 20,
        Mole = 21,
        ScoreGate = 22,
    }
    
    // Each value is one bit of a fixture's categoryBits/maskBits, so the highest usable value is 31.
    // Before adding a value, check whether an existing channel already expresses the same filtering
    // relationship - AnyObjectThatCollidesOnlyWithPlayer is shared by seven different body types.
    public enum PhysicsCollisionType
    {
        None = 0,
        PlayerSpaceship = 1,
        Wall = 2,
        PlayerBullet = 3,
        TalentCard = 4,
        PowerUpBall = 5,
        StartMatchWall = 6,
        AnyObjectThatCollidesOnlyWithPlayer = 7,
        KOProjectile = 8,
        GrapplingHookProjectile = 9,
        PlayerHeart = 10,
        ChickenEgg = 11,
        FrigidBlock = 12,
        FishingRodTip = 13,
        SoulGhost = 14,
        Mole = 15,
        ScoreGate = 16,
    }
}