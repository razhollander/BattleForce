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
using Core.Scripts.Utils;
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

        private readonly ConcurrentPool<BodyDef> _bodyDefPool;
        private readonly ConcurrentPool<FixtureDef> _fixtureDefPool;
        private readonly ConcurrentPool<PolygonShape> _polygonShapePool;
        private readonly ConcurrentPool<CircleShape> _circleShapePool;

        public PhysicsSimulator(IUpdateSubscriptionService updateSubscriptionService, NetworkConfig networkConfig)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _networkConfig = networkConfig;
            _collisionEventCacheListener = new CollisionEventCacheListener(_networkConfig);

            _bodyDefPool = new ConcurrentPool<BodyDef>(() => new BodyDef(), _networkConfig.MaxCap.ConcurrentBodyCount);
            _fixtureDefPool = new ConcurrentPool<FixtureDef>(() => new FixtureDef(), _networkConfig.MaxCap.ConcurrentFixuresCount);
            _polygonShapePool = new ConcurrentPool<PolygonShape>(() => new PolygonShape(), _networkConfig.MaxCap.ConcurrentPolygonCount);
            _circleShapePool = new ConcurrentPool<CircleShape>(() => new CircleShape(), _networkConfig.MaxCap.ConcurrentCircleCount);
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
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = Vector2.Zero; // Assume walls are absolute-world positioned
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.Wall);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var wallShape = GetPolygonShape();
            wallShape.Set(points);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = wallShape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.filter.categoryBits = PhysicsBodyType.Wall.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.Wall.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(wallShape);
        }

        public void AddLavaWall(ushort id, Vector2[] points)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = Vector2.Zero;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.Lava);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var lavaShape = GetPolygonShape();
            lavaShape.Set(points);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = lavaShape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.categoryBits = PhysicsBodyType.Lava.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.Lava.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(lavaShape);
        }

        public void AddPlayer(ushort id, ushort teamId, Vector2 position, Vector2 velocity, float radius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.position = position;
            bodyDef.linearVelocity = velocity;
            bodyDef.type = BodyType.Dynamic;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.PlayerSpaceship);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var circleShape = GetCircleShape();
            circleShape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = circleShape;
            fixtureDef.density = 1.0f;
            fixtureDef.friction = 0;
            fixtureDef.filter.categoryBits = PhysicsBodyType.PlayerSpaceship.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.PlayerSpaceship.GetCollisionMask();
            fixtureDef.filter.groupIndex = (short)-teamId;

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(circleShape);
        }

        public void AddPlayerBullet(ushort bulletId, ushort teamId, Vector2 bulletPosition, Vector2 bulletVelocity, float bulletRadius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.position = bulletPosition;
            bodyDef.linearVelocity = bulletVelocity;
            bodyDef.type = BodyType.Dynamic;
            bodyDef.bullet = true;
            bodyDef.userData = new PhysicsBodyData(bulletId, PhysicsBodyType.PlayerBullet);
            
            var bulletBody = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);
            
            CircleShape circleShape = GetCircleShape();
            circleShape.Radius = bulletRadius;
            
            FixtureDef fixtureDef = GetFixtureDef();
            fixtureDef.shape = circleShape;
            fixtureDef.density = 0.3f;
            fixtureDef.friction = 0.0f;
            fixtureDef.filter.categoryBits = PhysicsBodyType.PlayerBullet.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.PlayerBullet.GetCollisionMask();
            fixtureDef.filter.groupIndex = (short)-teamId;
            
            bulletBody.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(circleShape);
        }

        public void AddTalentCard(ushort id, Vector2 position, float length, float height)
        {
            BodyDef bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.TalentCard);

            Body body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            PolygonShape boxShape = GetPolygonShape();
            boxShape.SetAsBox(length * 0.5f, height * 0.5f);

            FixtureDef fixtureDef = GetFixtureDef();
            fixtureDef.shape = boxShape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.filter.categoryBits = PhysicsBodyType.TalentCard.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.TalentCard.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(boxShape);
        }

        public void AddPowerUpBall(ushort id, Vector2 position, Vector2 velocity, float radius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.linearVelocity = velocity;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.PowerUpBall);
            bodyDef.fixedRotation = true;

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var circleShape = GetCircleShape();
            circleShape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = circleShape;
            fixtureDef.density = 1f;
            fixtureDef.friction = 0;
            fixtureDef.restitution = 1f; // Bounciness
            fixtureDef.filter.categoryBits = PhysicsBodyType.PowerUpBall.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.PowerUpBall.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(circleShape);
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

        private BodyDef GetBodyDef()
        {
            var def = _bodyDefPool.Get();
            def.Reset();
            return def;
        }

        private FixtureDef GetFixtureDef()
        {
            var def = _fixtureDefPool.Get();
            def.Reset();
            return def;
        }

        private PolygonShape GetPolygonShape()
        {
            return _polygonShapePool.Get();
        }

        private CircleShape GetCircleShape()
        {
            var shape = _circleShapePool.Get();
            shape.Reset();
            return shape;
        }
    }
}
