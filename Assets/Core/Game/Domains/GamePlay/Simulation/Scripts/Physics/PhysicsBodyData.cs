namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public struct PhysicsBodyData
    {
        public ushort Id;
        public PhysicsBodyType PhysicsBodyType;

        public PhysicsBodyData(ushort id, PhysicsBodyType physicsBodyType)
        {
            Id = id;
            PhysicsBodyType = physicsBodyType;
        }

        public void Reset(ushort id, PhysicsBodyType physicsBodyType)
        {
            Id = id;
            PhysicsBodyType = physicsBodyType;
        }
    }

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