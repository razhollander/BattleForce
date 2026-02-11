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
                                    | GetCollisionMask(PhysicsBodyType.PlayerBullet)
                                    | GetCollisionMask(PhysicsBodyType.Lava)
                                    | GetCollisionMask(PhysicsBodyType.TeamFloor)
                                    | GetCollisionMask(PhysicsBodyType.StartMatchWall)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.PlayerBullet:
                    collisionMask = GetCollisionMask(PhysicsBodyType.Wall)
                                    | GetCollisionMask(PhysicsBodyType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsBodyType.TalentCard)
                                    | GetCollisionMask(PhysicsBodyType.PowerUpBall)
                                    | GetCollisionMask(PhysicsBodyType.StartMatchWall)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.Wall:
                    collisionMask = GetCollisionMask(PhysicsBodyType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsBodyType.PlayerBullet)
                                    | GetCollisionMask(PhysicsBodyType.PowerUpBall)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.TalentCard:
                    collisionMask = GetCollisionMask(PhysicsBodyType.PlayerBullet)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.Lava:
                    collisionMask = GetCollisionMask(PhysicsBodyType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.PowerUpBall:
                    collisionMask = GetCollisionMask(PhysicsBodyType.Wall)
                                    | GetCollisionMask(PhysicsBodyType.PlayerBullet)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.TeamFloor:
                    collisionMask = GetCollisionMask(PhysicsBodyType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.StartMatchWall:
                    collisionMask = GetCollisionMask(PhysicsBodyType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsBodyType.PlayerBullet)
                                    | GetCollisionMask(PhysicsBodyType.Caster);
                    break;
                case PhysicsBodyType.Caster:
                    collisionMask = 0xFFFF; // Collide with everything
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