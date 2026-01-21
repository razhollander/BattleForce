using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;
#if UNITY_EDITOR && PHYSICS_DEBUG_DRAW_ENABLED
using Box2D.NetStandard.Dynamics.World.Callbacks;
#endif
using Box2D.WorldTests;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public class PhysicsSimulator : IPhysicsSimulator, IGUIUpdatable
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly NetworkConfig _networkConfig;
        private World _world;
        private readonly CollisionEventCacheListener _collisionEventCacheListener;

        public PhysicsSimulator(IUpdateSubscriptionService updateSubscriptionService, NetworkConfig networkConfig)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _networkConfig = networkConfig;
            _collisionEventCacheListener = new CollisionEventCacheListener(_networkConfig);
        }

        public void InitEntryPoint()
        {
            _world = CreateWorld();
            _updateSubscriptionService.RegisterGuiUpdatable(this);
        }

        public void InitExitPoint()
        {
            _updateSubscriptionService.UnregisterGuiUpdatable(this);
        }

        public void CopyDataToSimulation(SimulationStateS2C simulationState)
        {
            CopyPlayersStates(simulationState);
            CopyBulletsStates(simulationState);
            CopyPowerUpsStates(simulationState);
        }

        public Body GetPlayer(ushort playerId)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData)currentBody.UserData;

                if (bodyData.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && bodyData.Id == playerId)
                {
                    return currentBody;
                }

                currentBody = currentBody.GetNext();
            }

            LogService.LogError($"Couldn't find player {playerId}");
            return null;
        }

        private void CopyPlayersStates(SimulationStateS2C simulationState)
        {
            foreach (var playerState in simulationState.Players.AsSpan())
            {
                var currentBody = _world.GetBodyList();

                while (currentBody != null)
                {
                    var bodyData = (PhysicsBodyData) currentBody.UserData;

                    if (bodyData.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && bodyData.Id == playerState.Id)
                    {
                        currentBody.SetTransform(playerState.Spaceship.Transform.Position, playerState.Spaceship.Transform.Direction.ToAngleRadians());
                        currentBody.SetLinearVelocity(playerState.Spaceship.Transform.Velocity);
                        break;
                    }

                    currentBody = currentBody.GetNext();
                }
            }
        }

        private void CopyPowerUpsStates(SimulationStateS2C simulationState)
        {
            foreach (var powerUp in simulationState.PowerUpBalls.AsSpan())
            {
                var powerUpBody = _world.GetBodyList();

                while (powerUpBody != null)
                {
                    var bodyData = (PhysicsBodyData) powerUpBody.UserData;

                    if (bodyData.PhysicsBodyType == PhysicsBodyType.PowerUpBall && bodyData.Id == powerUp.Id)
                    {
                        powerUpBody.SetTransform(powerUp.Position, 0);
                        powerUpBody.SetLinearVelocity(powerUp.Velocity);
                        break;
                    }

                    powerUpBody = powerUpBody.GetNext();
                }
            }
        }

        private void CopyBulletsStates(SimulationStateS2C simulationState)
        {
            foreach (var bullet in simulationState.Bullets.AsSpan())
            {
                var bulletBody = _world.GetBodyList();

                while (bulletBody != null)
                {
                    var bodyData = (PhysicsBodyData) bulletBody.UserData;

                    if (bodyData.PhysicsBodyType == PhysicsBodyType.PlayerBullet && bodyData.Id == bullet.Id)
                    {
                        bulletBody.SetTransform(bullet.Position, bullet.Direction.ToAngleRadians());
                        bulletBody.SetLinearVelocity(bullet.Velocity);

                        break;
                    }

                    bulletBody = bulletBody.GetNext();
                }
            }
        }

        public void Step(float deltaTime, int velocityIterations, int positionIterations)
        {
            _world.Step(deltaTime, velocityIterations, positionIterations);
        }

        public FixedUnorderedList<PhysicsCollisionEvent> GetCachedCollisions()
        {
            return _collisionEventCacheListener.Events;
        }

        public void ClearCachedCollisions()
        {
            _collisionEventCacheListener.Clear();
        }

        private World CreateWorld()
        {
            var gravity = new Vector2(0f, 0f);
            var world = new World(gravity, _collisionEventCacheListener, _networkConfig.MaxCap.ConcurrentTimeOfImpactContacts, _networkConfig.MaxCap.ConcurrentBodyCount, _networkConfig.MaxCap.ConcurrentContactCount, _networkConfig.MaxCap.ConcurrentJointCount);
            var testDebugDrawer = CreateTestDebugDrawer();
            world.SetDebugDraw(testDebugDrawer);
            return world;
        }

        private static TestDebugDrawer CreateTestDebugDrawer()
        {
            var testDebugDrawer = new TestDebugDrawer();
#if UNITY_EDITOR && PHYSICS_DEBUG_DRAW_ENABLED
            testDebugDrawer.AppendFlags(DrawFlags.Aabb);
            testDebugDrawer.AppendFlags(DrawFlags.Joint);
            testDebugDrawer.AppendFlags(DrawFlags.Pair);
            testDebugDrawer.AppendFlags(DrawFlags.Shape);
            testDebugDrawer.AppendFlags(DrawFlags.CenterOfMass);
#endif

            return testDebugDrawer;
        }

        public void SetPlayerVelocity(ushort playerId, Vector2 velocity)
        {
            GetPlayer(playerId).SetLinearVelocity(velocity);
        }

        public void AddWall(ushort id, Vector2[] points)
        {
            BodyDef bodyDef = new BodyDef 
            {
                type = BodyType.Static,
                position = Vector2.Zero, // Assume walls are absolute-world positioned
                userData = new PhysicsBodyData(id, PhysicsBodyType.Wall)
            };

            Body body = _world.CreateBody(bodyDef);

            PolygonShape wallShape = new PolygonShape();
            wallShape.Set(points);

            FixtureDef fixtureDef = new FixtureDef
            {
                shape = wallShape,
                density = 0,       // Static objects don't need density
                friction = 0,
                filter = new Filter
                {
                    categoryBits = PhysicsBodyType.Wall.GetCollisionsCategory(),
                    maskBits     = PhysicsBodyType.Wall.GetCollisionMask(),
                }
            };

            body.CreateFixture(fixtureDef);
        }

        public void AddLavaWall(ushort id, Vector2[] points)
        {
            BodyDef bodyDef = new BodyDef
            {
                type = BodyType.Static,
                position = Vector2.Zero,
                userData = new PhysicsBodyData(id, PhysicsBodyType.Lava)
            };

            Body body = _world.CreateBody(bodyDef);

            PolygonShape lavaShape = new PolygonShape();
            lavaShape.Set(points);

            FixtureDef fixtureDef = new FixtureDef
            {
                shape = lavaShape,
                density = 0,
                friction = 0,
                isSensor = true,
                filter = new Filter
                {
                    categoryBits = PhysicsBodyType.Lava.GetCollisionsCategory(),
                    maskBits     = PhysicsBodyType.Lava.GetCollisionMask(),
                }
            };

            body.CreateFixture(fixtureDef);
        }

        public void AddPlayer(ushort id, ushort teamId, Vector2 position, Vector2 velocity, float radius)
        {
            BodyDef bodyDef = new BodyDef
            {
                position = position,
                linearVelocity = velocity,
                type = BodyType.Dynamic,
                userData = new PhysicsBodyData(id, PhysicsBodyType.PlayerSpaceship)
            };

            Body body = _world.CreateBody(bodyDef);

            CircleShape circleShape = new CircleShape();
            circleShape.Radius = radius;

            FixtureDef fixtureDef = new FixtureDef
            {
                shape = circleShape,
                density = 1.0f,
                friction = 0,
                filter = new Filter
                {
                    categoryBits = PhysicsBodyType.PlayerSpaceship.GetCollisionsCategory(),
                    maskBits     = PhysicsBodyType.PlayerSpaceship.GetCollisionMask(),
                    groupIndex = (short)-teamId,
                }
            };

            body.CreateFixture(fixtureDef);
        }

        public void AddPlayerBullet(ushort bulletId, ushort teamId, Vector2 bulletPosition, Vector2 bulletVelocity, float bulletRadius)
        {
            BodyDef bodyDef = new BodyDef
            {
                position = bulletPosition,
                linearVelocity = bulletVelocity,
                type = BodyType.Dynamic,
                bullet = true,
                userData = new PhysicsBodyData(bulletId, PhysicsBodyType.PlayerBullet)
            };
            
            Body bulletBody = _world.CreateBody(bodyDef);
            
            CircleShape circleShape = new CircleShape
            {
                Radius = bulletRadius
            };
            
            FixtureDef fixtureDef = new FixtureDef
            {
                shape = circleShape,
                density = 0.3f,
                friction = 0.0f,
                filter = new Filter
                {
                    categoryBits = PhysicsBodyType.PlayerBullet.GetCollisionsCategory(),
                    maskBits = PhysicsBodyType.PlayerBullet.GetCollisionMask(),
                    groupIndex = (short)-teamId,
                },
            };
            
            bulletBody.CreateFixture(fixtureDef);
        }

        public void AddTalentCard(ushort id, Vector2 position, float length, float height)
        {
            BodyDef bodyDef = new BodyDef
            {
                type = BodyType.Static,
                position = position,
                userData = new PhysicsBodyData(id, PhysicsBodyType.TalentCard)
            };

            Body body = _world.CreateBody(bodyDef);

            PolygonShape boxShape = new PolygonShape();
            boxShape.SetAsBox(length * 0.5f, height * 0.5f);

            FixtureDef fixtureDef = new FixtureDef
            {
                shape = boxShape,
                density = 0,
                friction = 0,
                filter = new Filter
                {
                    categoryBits = PhysicsBodyType.TalentCard.GetCollisionsCategory(),
                    maskBits     = PhysicsBodyType.TalentCard.GetCollisionMask(),
                }
            };

            body.CreateFixture(fixtureDef);
        }

        public void AddPowerUpBall(ushort id, Vector2 position, Vector2 velocity, float radius)
        {
            BodyDef bodyDef = new BodyDef
            {
                type = BodyType.Dynamic,
                position = position,
                linearVelocity = velocity,
                userData = new PhysicsBodyData(id, PhysicsBodyType.PowerUpBall),
                fixedRotation = true
            };

            Body body = _world.CreateBody(bodyDef);

            CircleShape circleShape = new CircleShape();
            circleShape.Radius = radius;

            FixtureDef fixtureDef = new FixtureDef
            {
                shape = circleShape,
                density = 1f,
                friction = 0,
                restitution = 1f, // Bounciness
                filter = new Filter
                {
                    categoryBits = PhysicsBodyType.PowerUpBall.GetCollisionsCategory(),
                    maskBits     = PhysicsBodyType.PowerUpBall.GetCollisionMask(),
                }
            };

            body.CreateFixture(fixtureDef);
        }

        public Body GetBullet(ushort bulletId)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData) currentBody.UserData;

                if (bodyData.PhysicsBodyType == PhysicsBodyType.PlayerBullet && bodyData.Id == bulletId)
                {
                    return currentBody;
                }

                currentBody = currentBody.GetNext();
            }
            
            LogService.LogError($"Couldn't find bullet {bulletId}");
            return default;
        }

        public Body GetPowerUpBall(ushort powerUpBallId)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData) currentBody.UserData;

                if (bodyData.PhysicsBodyType == PhysicsBodyType.PowerUpBall && bodyData.Id == powerUpBallId)
                {
                    return currentBody;
                }

                currentBody = currentBody.GetNext();
            }

            LogService.LogError($"Couldn't find powerUp {powerUpBallId}");
            return default;
        }

        public void RemoveBody(Body body)
        {
            _world.DestroyBody(body);
        }

        public bool IsSquareHitAnyBodyTypes(Vector2 squarePosition, float squareHalfWidth, params PhysicsBodyType[] bodyTypes)
        {
            var hasCollisionWithAnyBodyType = false;
            var lowerBound = squarePosition - new Vector2(squareHalfWidth, squareHalfWidth);
            var upperBound = squarePosition + new Vector2(squareHalfWidth, squareHalfWidth);
            var aabb = new Box2D.NetStandard.Collision.AABB(lowerBound, upperBound);

            _world.QueryAABB(ShouldProceedCheckHit, aabb);

            bool ShouldProceedCheckHit(Fixture fixture)
            {
                var bodyData = (PhysicsBodyData)fixture.Body.UserData;

                for (int i = 0; i < bodyTypes.Length; i++)
                {
                    hasCollisionWithAnyBodyType = bodyData.PhysicsBodyType == bodyTypes[i];
                    if (hasCollisionWithAnyBodyType)
                    {
                        return false;
                    }
                }
                
                return true;
            }
            
            return hasCollisionWithAnyBodyType;
        }

        public void ManagedOnGUI()
        {
            
        }

        public void ManagedOnDrawGizmos()
        {
            _world?.DrawDebugData();
        }
    }
}