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
        PowerUpBall = 6
    }
}