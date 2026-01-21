namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    [System.Flags]
    public enum PhysicsBodyType
    {
        PlayerSpaceship = 0,
        Wall = 1,
        PlayerBullet = 2,
        TalentCard = 3,
        Lava = 4,
        PowerUpBall = 5
    }
}