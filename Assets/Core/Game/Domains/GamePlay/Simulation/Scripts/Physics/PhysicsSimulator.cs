using System.Collections.Generic;
using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Box2D.WorldTests;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public class PhysicsSimulator : IPhysicsSimulator, IGUIUpdatable
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private World _world;
        private CollisionEventCacheListener _collisionEventCacheListener;

        public PhysicsSimulator(IUpdateSubscriptionService updateSubscriptionService)
        {
            _updateSubscriptionService = updateSubscriptionService;
        }

        public void InitEntryPoint()
        {
            _world = CreateWorld();
            _collisionEventCacheListener = new CollisionEventCacheListener();
            _world.SetContactListener(_collisionEventCacheListener);
            var testDebugDrawer = new TestDebugDrawer();
            testDebugDrawer.AppendFlags(DrawFlags.Aabb);
            testDebugDrawer.AppendFlags(DrawFlags.Joint);
            testDebugDrawer.AppendFlags(DrawFlags.Pair);
            testDebugDrawer.AppendFlags(DrawFlags.Shape);
            testDebugDrawer.AppendFlags(DrawFlags.CenterOfMass);
            _world.SetDebugDraw(testDebugDrawer);
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
        }

        private void CopyPlayersStates(SimulationStateS2C simulationState)
        {
            var players = simulationState.Players;

            for (int i = 0; i < simulationState.PlayersCount; i++)
            {
                var playerState = players[i];
                var currentBody = _world.GetBodyList();

                while (currentBody != null)
                {
                    var bodyData = (PhysicsBodyData) currentBody.UserData;

                    if (bodyData.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && bodyData.Id == playerState.Id)
                    {
                        currentBody.SetTransform(playerState.Spaceship.Transform.Position, playerState.Spaceship.Transform.Direction.ToAngle());
                        currentBody.SetLinearVelocity(playerState.Spaceship.Transform.Velocity);

                        break;
                    }

                    currentBody = currentBody.GetNext();
                }
            }
        }

        private void CopyBulletsStates(SimulationStateS2C simulationState)
        {
            foreach (var bulletIndex in simulationState.Bullets.UsedIndices())
            {
                var bulletBody = _world.GetBodyList();
                var bullet = simulationState.Bullets[bulletIndex];

                while (bulletBody != null)
                {
                    var bodyData = (PhysicsBodyData) bulletBody.UserData;

                    if (bodyData.PhysicsBodyType == PhysicsBodyType.PlayerBullet && bodyData.Id == bullet.Id)
                    {
                        bulletBody.SetTransform(bullet.Position, bullet.Direction.ToAngle());
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

        public IReadOnlyList<PhysicsCollisionEvent> GetCachedCollisions()
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
            var world = new World(gravity);

            return world;
        }

        public void SetPlayerVelocity(int playerId, Vector2 velocity)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData)currentBody.UserData;
                bool isPlayer = bodyData.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && bodyData.Id == playerId;
                if (isPlayer)
                {
                    currentBody.SetLinearVelocity(velocity);
                    return;
                }

                currentBody = currentBody.GetNext();
            }
        }

        public void AddWall(int id, Vector2[] points)
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
        
        public void AddPlayer(int id, Vector2 position, Vector2 velocity, float radius)
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
                }
            };

            body.CreateFixture(fixtureDef);
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