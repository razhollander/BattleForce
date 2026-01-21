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
        private readonly ConcurrentPool<PhysicsBodyDataWrapper> _bodyDataWrapperPool;

        public PhysicsSimulator(IUpdateSubscriptionService updateSubscriptionService, NetworkConfig networkConfig)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _networkConfig = networkConfig;
            _collisionEventCacheListener = new CollisionEventCacheListener(_networkConfig);

            int bodyCount = _networkConfig.MaxCap.ConcurrentBodyCount;
            // Fixtures and shapes usually match body count or slightly more (walls have multiple segments? no, separate bodies).
            // Using bodyCount as a safe initial estimate.
            _bodyDefPool = new ConcurrentPool<BodyDef>(() => new BodyDef(), bodyCount);
            _fixtureDefPool = new ConcurrentPool<FixtureDef>(() => new FixtureDef(), bodyCount);
            _polygonShapePool = new ConcurrentPool<PolygonShape>(() => new PolygonShape(), bodyCount);
            _circleShapePool = new ConcurrentPool<CircleShape>(() => new CircleShape(), bodyCount);
            _bodyDataWrapperPool = new ConcurrentPool<PhysicsBodyDataWrapper>(() => new PhysicsBodyDataWrapper(), bodyCount);
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
                var bodyData = ((PhysicsBodyDataWrapper)currentBody.UserData).Data;

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
                    var bodyData = ((PhysicsBodyDataWrapper) currentBody.UserData).Data;

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
                    var bodyData = ((PhysicsBodyDataWrapper) powerUpBody.UserData).Data;

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
                    var bodyData = ((PhysicsBodyDataWrapper) bulletBody.UserData).Data;

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
            BodyDef bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = Vector2.Zero; // Assume walls are absolute-world positioned
            bodyDef.userData = GetBodyDataWrapper(id, PhysicsBodyType.Wall);

            Body body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            PolygonShape wallShape = GetPolygonShape();
            wallShape.Set(points);

            FixtureDef fixtureDef = GetFixtureDef();
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
            BodyDef bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = Vector2.Zero;
            bodyDef.userData = GetBodyDataWrapper(id, PhysicsBodyType.Lava);

            Body body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            PolygonShape lavaShape = GetPolygonShape();
            lavaShape.Set(points);

            FixtureDef fixtureDef = GetFixtureDef();
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
            BodyDef bodyDef = GetBodyDef();
            bodyDef.position = position;
            bodyDef.linearVelocity = velocity;
            bodyDef.type = BodyType.Dynamic;
            bodyDef.userData = GetBodyDataWrapper(id, PhysicsBodyType.PlayerSpaceship);

            Body body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            CircleShape circleShape = GetCircleShape();
            circleShape.Radius = radius;

            FixtureDef fixtureDef = GetFixtureDef();
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
            BodyDef bodyDef = GetBodyDef();
            bodyDef.position = bulletPosition;
            bodyDef.linearVelocity = bulletVelocity;
            bodyDef.type = BodyType.Dynamic;
            bodyDef.bullet = true;
            bodyDef.userData = GetBodyDataWrapper(bulletId, PhysicsBodyType.PlayerBullet);
            
            Body bulletBody = _world.CreateBody(bodyDef);
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
            bodyDef.userData = GetBodyDataWrapper(id, PhysicsBodyType.TalentCard);

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
            BodyDef bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.linearVelocity = velocity;
            bodyDef.userData = GetBodyDataWrapper(id, PhysicsBodyType.PowerUpBall);
            bodyDef.fixedRotation = true;

            Body body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            CircleShape circleShape = GetCircleShape();
            circleShape.Radius = radius;

            FixtureDef fixtureDef = GetFixtureDef();
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
                var bodyData = ((PhysicsBodyDataWrapper) currentBody.UserData).Data;

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
                var bodyData = ((PhysicsBodyDataWrapper) currentBody.UserData).Data;

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
            if (body.UserData is PhysicsBodyDataWrapper wrapper)
            {
                _bodyDataWrapperPool.Return(wrapper);
                body.UserData = null;
            }
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
                var bodyData = ((PhysicsBodyDataWrapper)fixture.Body.UserData).Data;

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
            // Reset to defaults as per BodyDef constructor
            def.userData = null;
            def.position = Vector2.Zero;
            def.angle = 0.0f;
            def.linearVelocity = Vector2.Zero;
            def.angularVelocity = 0.0f;
            def.linearDamping = 0.0f;
            def.angularDamping = 0.0f;
            def.allowSleep = true;
            def.awake = true;
            def.fixedRotation = false;
            def.bullet = false;
            def.type = BodyType.Static;
            def.enabled = true;
            def.gravityScale = 1.0f;
            return def;
        }

        private FixtureDef GetFixtureDef()
        {
            var def = _fixtureDefPool.Get();
            // Reset to defaults
            def.density = 0f;
            def.friction = 0.2f;
            def.isSensor = false;
            def.restitution = 0f;
            def.shape = null;
            def.userData = null;
            // Reset filter
            if (def.filter != null)
            {
                def.filter.categoryBits = 0x0001;
                def.filter.maskBits = 0xFFFF;
                def.filter.groupIndex = 0;
            }
            else
            {
                // Should not happen if pool generator uses new FixtureDef() which initializes filter
                def.filter = new Filter();
            }
            return def;
        }

        private PolygonShape GetPolygonShape()
        {
            var shape = _polygonShapePool.Get();
            // PolygonShape doesn't have a clear reset method but Set() methods will overwrite data.
            // m_count = 0 in Set methods or constructor.
            // Vertices are overwritten.
            // We should ensure it's clean enough.
            // Using SetAsBox(0,0) might be a way to reset, but not strictly necessary if we always call Set/SetAsBox immediately.
            return shape;
        }

        private CircleShape GetCircleShape()
        {
            var shape = _circleShapePool.Get();
            shape.Radius = 0;
            shape.Center = Vector2.Zero;
            return shape;
        }

        private PhysicsBodyDataWrapper GetBodyDataWrapper(ushort id, PhysicsBodyType type)
        {
            var wrapper = _bodyDataWrapperPool.Get();
            wrapper.Reset(id, type);
            return wrapper;
        }
    }
}
