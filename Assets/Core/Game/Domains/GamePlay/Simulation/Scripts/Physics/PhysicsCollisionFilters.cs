namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public static class PhysicsCollisionFilters
    {
        // Box2D collides two fixtures only when BOTH directions pass:
        //   (A.categoryBits & B.maskBits) != 0  AND  (B.categoryBits & A.maskBits) != 0
        //
        // GetCollisionsCategory builds a fixture's categoryBits: the SET of collision bits this body
        // exposes, i.e. "which other bodies' masks are allowed to see me". It ORs several single bits
        // together, so it returns a compound mask (e.g. a Wall is seen by players, bullets, powerups...).
        public static ushort GetCollisionsCategory(this PhysicsBodyType type)
        {
            int collisionMask;

            switch (type)
            {
                case PhysicsBodyType.PlayerSpaceship:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.StartMatchWall)
                                    | GetCollisionMask(PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer)
                                    | GetCollisionMask(PhysicsCollisionType.KOProjectile)
                                    | GetCollisionMask(PhysicsCollisionType.ChickenEgg)
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
                                    | GetCollisionMask(PhysicsCollisionType.FishingRodTip)
                                    | GetCollisionMask(PhysicsCollisionType.Mole)
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
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
                                    | GetCollisionMask(PhysicsCollisionType.Mole)
                                    | GetCollisionMask(PhysicsCollisionType.StartMatchWall);
                    break;
                case PhysicsBodyType.Wall:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet)
                                    | GetCollisionMask(PhysicsCollisionType.PowerUpBall)
                                    | GetCollisionMask(PhysicsCollisionType.KOProjectile)
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
                                    | GetCollisionMask(PhysicsCollisionType.GrapplingHookProjectile)
                                    | GetCollisionMask(PhysicsCollisionType.FishingRodTip)
                                    | GetCollisionMask(PhysicsCollisionType.SoulGhost);
                    break;
                case PhysicsBodyType.FrigidBlock:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet)
                                    | GetCollisionMask(PhysicsCollisionType.PowerUpBall)
                                    | GetCollisionMask(PhysicsCollisionType.KOProjectile)
                                    | GetCollisionMask(PhysicsCollisionType.GrapplingHookProjectile)
                                    | GetCollisionMask(PhysicsCollisionType.FishingRodTip)
                                    | GetCollisionMask(PhysicsCollisionType.SoulGhost)
                                    | GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock);
                    break;
                case PhysicsBodyType.TalentCard:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerBullet);
                    break;
                case PhysicsBodyType.Lava:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.PowerUpBall:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
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
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
                                    | GetCollisionMask(PhysicsCollisionType.ChickenEgg);
                    break;
                case PhysicsBodyType.SwapField:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.StageBoundary:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.GrapplingHookProjectile:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock);
                    break;
                case PhysicsBodyType.FishingRodTip:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                case PhysicsBodyType.SoulGhost:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.Wall)
                                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock);
                    break;
                case PhysicsBodyType.ChickenEgg:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                                    | GetCollisionMask(PhysicsCollisionType.KOProjectile);
                    break;
                // A mole is a sensor that only needs to notice bullets and spaceships - those are the two ways it can be whacked.
                case PhysicsBodyType.Mole:
                    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerBullet)
                                    | GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
                    break;
                default:
                    collisionMask = 0xFFFF;
                    break;
            }

            return (ushort) collisionMask;
        }

        // GetCollisionMask builds a SINGLE collision bit from one PhysicsCollisionType.
        // Used to set a fixture's maskBits ("who I collide with") and to OR individual
        // bits into the category set above. 
        public static ushort GetCollisionMask(this PhysicsCollisionType type)
        {
            return (ushort) (1 << (int) type);
        }
    }
}