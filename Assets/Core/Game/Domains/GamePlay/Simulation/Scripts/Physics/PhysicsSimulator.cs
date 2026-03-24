using System;
using System.Numerics;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;
#if UNITY_EDITOR && DEBUG_DRAW_ENABLED
using Box2D.NetStandard.Dynamics.World.Callbacks;
#endif
using Box2D.WorldTests;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Extensions;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Network;
using Core.Scripts.Services.UnityThreadDispatcher;
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
        private readonly IUnityMainThreadDispatcher _unityMainThreadDispatcher;
        private World _world;
        private readonly CollisionEventCacheListener _collisionEventCacheListener;

        private readonly ConcurrentPool<BodyDef> _bodyDefPool;
        private readonly ConcurrentPool<FixtureDef> _fixtureDefPool;
        private readonly ConcurrentPool<PolygonShape> _polygonShapePool;
        private readonly ConcurrentPool<CircleShape> _circleShapePool;
        private readonly ConcurrentPool<Filter> _filterPool;

        public PhysicsSimulator(IUpdateSubscriptionService updateSubscriptionService, NetworkConfig networkConfig, IUnityMainThreadDispatcher unityMainThreadDispatcher)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _networkConfig = networkConfig;
            _unityMainThreadDispatcher = unityMainThreadDispatcher;
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

        public void CopyDataToSimulation(MatchSimulationStateS2C simulationState, 
            FixedClassUnorderedList<EnvironmentWallS2C> environmentWalls,
            FixedClassUnorderedList<EnvironmentWallS2C> environmentLavaWalls, 
            FixedClassUnorderedList<EnvironmentSpringS2C> environmentSprings,
            FixedClassUnorderedList<EnvironmentTeleportGatePairS2C> environmentTeleportGates)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData) currentBody.UserData;

                switch (bodyData.PhysicsBodyType)
                {
                    case PhysicsBodyType.PlayerSpaceship: CopyPlayerStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.PlayerBullet: CopyBulletStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.PowerUpBall: CopyPowerUpStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.Wall: CopyWallStateToBody(currentBody, bodyData.Id, environmentWalls); break;
                    case PhysicsBodyType.Lava: CopyLavaStateToBody(currentBody, bodyData.Id, environmentLavaWalls); break;
                    case PhysicsBodyType.EnvironmentSpring: CopySpringStateToBody(currentBody, bodyData.Id, environmentSprings); break;
                    case PhysicsBodyType.EnvironmentTeleportGate: CopyTeleportGateStateToBody(currentBody, bodyData.Id, environmentTeleportGates); break;
                    case PhysicsBodyType.SwapField: CopySwapFieldToBody(currentBody, bodyData.Id, simulationState); break;
                }

                currentBody = currentBody.GetNext();
            }
        }

        private void CopySwapFieldToBody(Body swapFieldBody, ushort swapFieldId, MatchSimulationStateS2C simulationState)
        {
            // var swapField = simulationState.Players.FindWithId(swapFieldId);
            // swapFieldBody.SetTransform(playerState.Spaceship.Transform.Position, playerState.Spaceship.Transform.Direction.ToAngleRadians());
            // swapFieldBody.set(playerState.Spaceship.Transform.Velocity);
        }

        private void CopyPowerUpStateToBody(Body powerUpBody, ushort powerUpId, MatchSimulationStateS2C simulationState)
        {
            var powerUpState = simulationState.PowerUpBalls.FindWithId(powerUpId);
            powerUpBody.SetTransform(powerUpState.Position, 0);
            powerUpBody.SetLinearVelocity(powerUpState.Velocity);
        }

        private void CopyBulletStateToBody(Body bulletBody, ushort bulletId, MatchSimulationStateS2C simulationState)
        {
            var bulletState = simulationState.Bullets.FindWithId(bulletId);
            bulletBody.SetTransform(bulletState.Position, bulletState.Direction.ToAngleRadians());
            bulletBody.SetLinearVelocity(bulletState.Velocity);
        }

        private void CopySpringStateToBody(Body springBody, ushort springId, FixedClassUnorderedList<EnvironmentSpringS2C> environmentSprings)
        {
            var environmentSpring = environmentSprings.FindWithId(springId);
            springBody.SetTransform(environmentSpring.Transform.WorldPosition, environmentSpring.Transform.WorldRotationDegrees.ToRadians());       
        }

        private void CopyTeleportGateStateToBody(Body teleportGateBody, ushort teleportGateId, FixedClassUnorderedList<EnvironmentTeleportGatePairS2C> environmentTeleportGates)
        {
            foreach (var teleportGatePair in environmentTeleportGates.AsSpan())
            {
                if (teleportGatePair.GateA.Id == teleportGateId)
                {
                    teleportGateBody.SetTransform(teleportGatePair.GateA.Transform.WorldPosition, teleportGatePair.GateA.Transform.WorldRotationDegrees.ToRadians());
                    return;
                }

                if (teleportGatePair.GateB.Id == teleportGateId)
                {
                    teleportGateBody.SetTransform(teleportGatePair.GateB.Transform.WorldPosition, teleportGatePair.GateB.Transform.WorldRotationDegrees.ToRadians());
                    return;
                }
            }

            throw new Exception("No teleport gate pair found for gate id: " + teleportGateId);
        }

        private void CopyLavaStateToBody(Body lavaBody, ushort lavaWallId, FixedClassUnorderedList<EnvironmentWallS2C> environmentLavaWalls)
        {
            var environmentLavaWall = environmentLavaWalls.FindWithId(lavaWallId);
            lavaBody.SetTransform(environmentLavaWall.Transform.WorldPosition, environmentLavaWall.Transform.WorldRotationDegrees.ToRadians());
        }

        private void CopyWallStateToBody(Body wallBody, ushort wallId, FixedClassUnorderedList<EnvironmentWallS2C> environmentWalls)
        {
            var environmentWall = environmentWalls.FindWithId(wallId);
            wallBody.SetTransform(environmentWall.Transform.WorldPosition, environmentWall.Transform.WorldRotationDegrees.ToRadians());
        }

        private void CopyPlayerStateToBody(Body playerBody, ushort playerId, MatchSimulationStateS2C simulationState)
        {
            var playerState = simulationState.Players.FindWithId(playerId);
            playerBody.SetTransform(playerState.Spaceship.Transform.Position, playerState.Spaceship.Transform.Direction.ToAngleRadians());
            playerBody.SetLinearVelocity(playerState.Spaceship.Transform.Velocity);
        }

        public void CopyDataToSimulation(MatchMakingSimulationStateS2C simulationState)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData) currentBody.UserData;

                switch (bodyData.PhysicsBodyType)
                {
                    case PhysicsBodyType.PlayerSpaceship: CopyPlayerStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.PlayerBullet: CopyBulletStateToBody(currentBody, bodyData.Id, simulationState); break;
                }

                currentBody = currentBody.GetNext();
            }
        }
        
        private void CopyPlayerStateToBody(Body playerBody, ushort playerId, MatchMakingSimulationStateS2C simulationState)
        {
            var playerState = simulationState.Players.FindWithId(playerId);
            playerBody.SetTransform(playerState.Spaceship.Transform.Position, playerState.Spaceship.Transform.Direction.ToAngleRadians());
            playerBody.SetLinearVelocity(playerState.Spaceship.Transform.Velocity);
        }
        
        private void CopyBulletStateToBody(Body bulletBody, ushort bulletId, MatchMakingSimulationStateS2C simulationState)
        {
            var bulletState = simulationState.Bullets.FindWithId(bulletId);
            bulletBody.SetTransform(bulletState.Position, bulletState.Direction.ToAngleRadians());
            bulletBody.SetLinearVelocity(bulletState.Velocity);
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
#if UNITY_EDITOR && DEBUG_DRAW_ENABLED
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

        public void AddWall(ushort id, Vector2[] points, Vector2 position)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
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

        public void AddLavaWall(ushort id, Vector2[] points, Vector2 position)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
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

        public void AddTeamFloor(ushort id, Vector2[] points, Vector2 position)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.TeamFloor);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetPolygonShape();
            shape.Set(points);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.categoryBits = PhysicsBodyType.TeamFloor.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.TeamFloor.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(shape);
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
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = bulletPosition;
            bodyDef.linearVelocity = bulletVelocity;
            bodyDef.bullet = true;
            bodyDef.fixedRotation = true;
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
            bodyDef.fixedRotation = true;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.PowerUpBall);

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
            var aabb = new AABB(lowerBound, upperBound);

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

        public bool CircleCast(Vector2 center, float radius, params PhysicsBodyType[] bodyTypes)
        {
            var hasCollision = false;
            var lowerBound = center - new Vector2(radius, radius);
            var upperBound = center + new Vector2(radius, radius);
            var aabb = new AABB(lowerBound, upperBound);

            _world.QueryAABB(fixture =>
            {
                var bodyData = (PhysicsBodyData)fixture.Body.UserData;

                for (int i = 0; i < bodyTypes.Length; i++)
                {
                    if (bodyData.PhysicsBodyType == bodyTypes[i])
                    {
                        var circleShape = GetCircleShape();
                        circleShape.Radius = radius;
                        circleShape.Center = Vector2.Zero;

                        var input = new ShapeCastInput();
                        input.proxyA.Set(circleShape, 0);
                        input.proxyB.Set(fixture.Shape, 0);
                        input.transformA = new Transform(center, Matrix3x2.Identity);
                        input.transformB = fixture.Body.GetTransform();
                        input.translationB = Vector2.Zero;

                        if (Contact.ShapeCast(out _, input)) // todo: this generates garbage inside, need to pool Symplex
                        {
                            hasCollision = true;
                        }

                        _circleShapePool.Return(circleShape);
                        return !hasCollision;
                    }
                }

                return true;
            }, aabb);

            return hasCollision;
        }

        public bool RectangleCast(Vector2 center, Vector2 size, float angleRadians, params PhysicsBodyType[] bodyTypes)
        {
            _unityMainThreadDispatcher.EnqueueDraw(()=>DebugDrawUtils.DrawRotatedRect(center, size, angleRadians));
            var hasCollision = false;

            var hx = size.X * 0.5f;
            var hy = size.Y * 0.5f;

            var rot = Matrix3x2.CreateRotation(angleRadians);
            var v1 = Vector2.Transform(new Vector2(-hx, -hy), rot) + center;
            var v2 = Vector2.Transform(new Vector2(hx, -hy), rot) + center;
            var v3 = Vector2.Transform(new Vector2(hx, hy), rot) + center;
            var v4 = Vector2.Transform(new Vector2(-hx, hy), rot) + center;

            var min = Vector2.Min(Vector2.Min(v1, v2), Vector2.Min(v3, v4));
            var max = Vector2.Max(Vector2.Max(v1, v2), Vector2.Max(v3, v4));

            var aabb = new AABB(min, max);

            _world.QueryAABB(fixture =>
            {
                var bodyData = (PhysicsBodyData)fixture.Body.UserData;

                for (int i = 0; i < bodyTypes.Length; i++)
                {
                    if (bodyData.PhysicsBodyType == bodyTypes[i])
                    {
                        var polygonShape = GetPolygonShape();
                        polygonShape.SetAsBox(hx, hy);

                        var input = new ShapeCastInput();
                        input.proxyA.Set(polygonShape, 0);
                        input.proxyB.Set(fixture.Shape, 0);
                        input.transformA = new Transform(center, rot);
                        input.transformB = fixture.Body.GetTransform();
                        input.translationB = Vector2.Zero;

                        if (Contact.ShapeCast(out _, input))
                        {
                            hasCollision = true;
                        }

                        _polygonShapePool.Return(polygonShape);
                        return !hasCollision;
                    }
                }

                return true;
            }, aabb);

            return hasCollision;
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
            // var polygonShape = _polygonShapePool.Get();
            // polygonShape.Reset();
            // return polygonShape;
            
            //todo:
            //1. Pool any Body we create (it is created at World.CreateBody())
            //2. Return the Body to the pool when it is destroyed
            //3. Once the Body is destroyed, also return the Poylgon shape attached to it to its pool
            //Why? Polygon shape has 2 arrays (m_normals, m_vertices) which are being used by the physics engine. Therefore, we need to return the Polygon to the pool only when the Body holding it is destroyed.
            return new PolygonShape();
        }

        private CircleShape GetCircleShape()
        {
            var shape = _circleShapePool.Get();
            shape.Reset();
            return shape;
        }

        public void AddStartMatchWall(ushort id, Vector2 position, float radius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.StartMatchWall);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var circleShape = GetCircleShape();
            circleShape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = circleShape;
            fixtureDef.density = 0f;
            fixtureDef.friction = 0;
            fixtureDef.filter.categoryBits = PhysicsBodyType.StartMatchWall.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.StartMatchWall.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(circleShape);
        }

        public void AddEnvironmentSpring(ushort id, Vector2 position, float rotationDegrees, Vector2 size)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.angle = rotationDegrees.ToRadians();
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.EnvironmentSpring);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetPolygonShape();
            shape.SetAsBox(size.X * 0.5f, size.Y * 0.5f);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;

            fixtureDef.filter.categoryBits = PhysicsBodyType.EnvironmentSpring.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.EnvironmentSpring.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(shape);
        }

        public void AddTeleportGate(ushort id, Vector2 position, float rotation, Vector2 size)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.angle = rotation.ToRadians();
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.EnvironmentTeleportGate);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetPolygonShape();
            shape.SetAsBox(size.X * 0.5f, size.Y * 0.5f);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;

            fixtureDef.filter.categoryBits = PhysicsBodyType.EnvironmentTeleportGate.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.EnvironmentTeleportGate.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(shape);
        }

        public void AddKOProjectile(ushort id, ushort teamId, Vector2 position, float radius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.KOProjectile);
            bodyDef.bullet = true;

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);
            
            var shape = GetCircleShape();
            shape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0.3f;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.groupIndex = (short)-teamId;
            fixtureDef.filter.categoryBits = PhysicsBodyType.KOProjectile.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.KOProjectile.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(shape);
        }

        public void UpdateKOProjectile(ushort id, Vector2 position)
        {
            var body = GetBody(PhysicsBodyType.KOProjectile, id);
            body.SetTransform(position, 0);
        }


        public void RemoveKOProjectile(ushort id)
        {
            var body = GetBody(PhysicsBodyType.KOProjectile, id);
            RemoveBody(body);
        }

        public void AddSwapField(ushort id, ushort teamId, Vector2 position)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.SwapField);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetCircleShape();
            shape.Radius = 0;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.groupIndex = (short)-teamId;

            fixtureDef.filter.categoryBits = PhysicsBodyType.SwapField.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsBodyType.SwapField.GetCollisionMask();

            body.CreateFixture(fixtureDef);

            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(shape);
        }

        public void UpdateSwapField(ushort id, Vector2 position, float newRadius)
        {
            var swapFieldBody = GetBody(PhysicsBodyType.SwapField, id);
            swapFieldBody.SetTransform(position, swapFieldBody.GetAngle());
            var fixture = swapFieldBody.GetFixtureList();
            var shape = (CircleShape) fixture.Shape;
            shape.Radius = newRadius;
        }

        public Body GetBody(PhysicsBodyType bodyType, ushort bodyId)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData) currentBody.UserData;

                if (bodyData.PhysicsBodyType == bodyType && bodyData.Id == bodyId)
                {
                    return currentBody;
                }

                currentBody = currentBody.GetNext();
            }
            
            LogService.LogError($"Couldn't find bodyType {bodyType}, bodyId {bodyId}");
            return default;
        }

        public void RemoveSwapField(ushort id)
        {
            var body = GetBody(PhysicsBodyType.SwapField, id);
            _world.DestroyBody(body);
            LogService.LogError($"Physics RemoveSwapField {id}");
        }
        
        public void ClearAllData()
        {
            _world = CreateWorld();
            ClearCachedCollisions();
        }

        public void DisableBodyCollider(PhysicsBodyType koProjectile, ushort projectileId)
        {
            var body = GetBody(koProjectile, projectileId);
            var fixture = body.GetFixtureList();
            var filter = fixture.FilterData;
            filter.maskBits = 0x0000; 
            fixture.FilterData = filter; // not sure needed
            body.SetAwake(true); // not sure needed
        }
    }
}
