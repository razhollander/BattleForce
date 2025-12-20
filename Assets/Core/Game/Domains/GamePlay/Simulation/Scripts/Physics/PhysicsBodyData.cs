namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public struct PhysicsBodyData
    {
        public int Id;
        public PhysicsBodyType PhysicsBodyType;

        public PhysicsBodyData(int id, PhysicsBodyType physicsBodyType)
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
        Bullet = 2
    }
}