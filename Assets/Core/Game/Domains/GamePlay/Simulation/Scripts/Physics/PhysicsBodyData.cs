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

    public enum PhysicsBodyType
    {
        PlayerSpaceship,
        Wall,
        Bullet
    }
}