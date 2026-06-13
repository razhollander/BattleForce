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
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.StartMatchWall)
                                    | GetCollisionMask(PhysicsCollisionType.CollideOnlyWithPlayer)
                                    | GetCollisionMask(PhysicsCollisionType.KOProjectile)
                                    | GetCollisionMask(PhysicsCollisionType.ChickenEgg)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet);
                    break;
                case PhysicsBodyType.PlayerHeart:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerBullet);
                    break;
                case PhysicsBodyType.PlayerBullet:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerHeart)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.TalentCard)
                                    | GetCollisionMask(PhysicsCollisionType.PowerUpBall)
                                    | GetCollisionMask(PhysicsCollisionType.StartMatchWall);
                    break;
                case PhysicsBodyType.Wall:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet)
                                    | GetCollisionMask(PhysicsCollisionType.PowerUpBall)
                                    | GetCollisionMask(PhysicsCollisionType.KOProjectile)
                                    | GetCollisionMask(PhysicsCollisionType.GrapplingHookProjectile);
                    break;
                case PhysicsBodyType.TalentCard:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerBullet);
                    break;
                case PhysicsBodyType.Lava:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.PowerUpBall:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet);
                    break;
                case PhysicsBodyType.TeamFloor:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.StartMatchWall:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet);
                    break;
                case PhysicsBodyType.EnvironmentSpring:
                case PhysicsBodyType.EnvironmentSpike:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.EnvironmentTeleportGate:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.KOProjectile:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.ChickenEgg);
                    break;
                case PhysicsBodyType.SwapField:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.StageBoundary:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.GrapplingHookProjectile:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall);
                    break;
                case PhysicsBodyType.ChickenEgg:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.KOProjectile);
                    break;
                default:
                    collisionMask = 0xFFFF;
                    break;
            }

            return (ushort) collisionMask;
        }

        public static ushort GetCollisionMask(this PhysicsCollisionType type)
        {
            return (ushort) (1 << (int) type);
        }
    }
}