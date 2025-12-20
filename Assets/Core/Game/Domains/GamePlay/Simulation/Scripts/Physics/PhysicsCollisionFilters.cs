namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public static class PhysicsCollisionFilters
    {
        public static ushort GetCollisionsCategory(this PhysicsBodyType type)
        {
            int collisionMask;

            switch (type)
            {
                case PhysicsBodyType.PlayerSpaceship:
                    collisionMask = GetCollisionMask(PhysicsBodyType.Wall)
                                    | GetCollisionMask(PhysicsBodyType.Bullet);
                    break;
                case PhysicsBodyType.Bullet:
                    collisionMask = GetCollisionMask(PhysicsBodyType.Wall)
                                    | GetCollisionMask(PhysicsBodyType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.Wall:
                    collisionMask = GetCollisionMask(PhysicsBodyType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsBodyType.Bullet);
                    break;
                default:
                    collisionMask = 0xFFFF;
                    break;
            }

            return (ushort) collisionMask;
        }

        public static ushort GetCollisionMask(this PhysicsBodyType type)
        {
            return (ushort) (1 << (int) type);
        }
    }
}