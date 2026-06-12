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
        EnvironmentTeleportGate = 10,
        SwapField = 11,
        KOProjectile = 12,
        GrapplingHookProjectile = 13,
        StageBoundary = 14,
        PlayerHeart = 15,
        ChickenEgg = 16,
        RockWall = 17,
    }
    
    public enum PhysicsCollisionType
    {
        None = 0,
        PlayerSpaceship = 1,
        Wall = 2,
        PlayerBullet = 3,
        TalentCard = 4,
        PowerUpBall = 5,
        StartMatchWall = 6,
        CollideOnlyWithPlayer = 7,
        KOProjectile = 8,
        GrapplingHookProjectile = 9,
        PlayerHeart = 10,
        ChickenEgg = 11,
    }
}