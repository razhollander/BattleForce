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
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
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
        private const float DEFAULT_PLAYER_DENSITY = 1.0f;
        private const float DEFAULT_PLAYER_RESTITUTION = 0f;
        
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly NetworkConfig _networkConfig;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IUnityMainThreadDispatcher _unityMainThreadDispatcher;
        private World _world;
        private readonly CollisionEventCacheListener _collisionEventCacheListener;

        private readonly ConcurrentPool<BodyDef> _bodyDefPool;
        private readonly ConcurrentPool<FixtureDef> _fixtureDefPool;
        private readonly ConcurrentPool<PolygonShape> _polygonShapePool;
        private readonly ConcurrentPool<CircleShape> _circleShapePool;
        private readonly ConcurrentPool<Filter> _filterPool;

        public PhysicsSimulator(IUpdateSubscriptionService updateSubscriptionService, NetworkConfig networkConfig, ISimulationGamePlayConfigService gamePlayConfigService, IUnityMainThreadDispatcher unityMainThreadDispatcher)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _networkConfig = networkConfig;
            _gamePlayConfigService = gamePlayConfigService;
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
            FixedClassUnorderedList<EnvironmentSpikeS2C> environmentSpikes,
            FixedClassUnorderedList<EnvironmentTeleportGatePairS2C> environmentTeleportGates)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData) currentBody.UserData;

                switch (bodyData.PhysicsBodyType)
                {
                    case PhysicsBodyType.PlayerSpaceship: CopyPlayerStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.PlayerHeart: CopyPlayerHeartStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.PlayerBullet: CopyBulletStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.PowerUpBall: CopyPowerUpStateToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.Wall: CopyWallStateToBody(currentBody, bodyData.Id, environmentWalls); break;
                    case PhysicsBodyType.Lava: CopyLavaStateToBody(currentBody, bodyData.Id, environmentLavaWalls); break;
                    case PhysicsBodyType.EnvironmentSpring: CopySpringStateToBody(currentBody, bodyData.Id, environmentSprings); break;
                    case PhysicsBodyType.EnvironmentSpike: CopySpikeStateToBody(currentBody, bodyData.Id, environmentSpikes); break;
                    case PhysicsBodyType.EnvironmentTeleportGate: CopyTeleportGateStateToBody(currentBody, bodyData.Id, environmentTeleportGates); break;
                    case PhysicsBodyType.SwapField: CopySwapFieldToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.KOProjectile: CopyKOProjectileToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.GrapplingHookProjectile: CopyGrapplingHookProjectileToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.FishingRodTip: CopyFishingRodTipToBody(currentBody, bodyData.Id, simulationState); break;
                    case PhysicsBodyType.SoulGhost: CopySoulGhostToBody(currentBody, bodyData.Id, simulationState); break;
                }

                currentBody = currentBody.GetNext();
            }
        }

        private void CopyKOProjectileToBody(Body koProjectileBody, ushort koProjectileId, MatchSimulationStateS2C simulationState)
        {
            var koProjectileState = simulationState.KOProjectiles.FindWithId(koProjectileId);
            koProjectileBody.SetTransform(koProjectileState.Position, koProjectileState.Rotation.ToAngleRadians());
            koProjectileBody.SetLinearVelocity(koProjectileState.Velocity);
        }

        private void CopyGrapplingHookProjectileToBody(Body grapplingHookProjectileBody, ushort grapplingHookProjectileId, MatchSimulationStateS2C simulationState)
        {
            var grapplingHookProjectileState = simulationState.GrapplingHookProjectiles.FindWithId(grapplingHookProjectileId);
            grapplingHookProjectileBody.SetTransform(grapplingHookProjectileState.Position, 0);
            grapplingHookProjectileBody.SetLinearVelocity(grapplingHookProjectileState.Velocity);
        }

        private void CopyFishingRodTipToBody(Body fishingRodTipBody, ushort fishingRodTipId, MatchSimulationStateS2C simulationState)
        {
            var fishingRodTipState = simulationState.FishingRodProjectiles.FindWithId(fishingRodTipId);
            fishingRodTipBody.SetTransform(fishingRodTipState.Position, 0);
            fishingRodTipBody.SetLinearVelocity(fishingRodTipState.Velocity);
        }

        private void CopySoulGhostToBody(Body soulGhostBody, ushort soulGhostId, MatchSimulationStateS2C simulationState)
        {
            var soulGhostState = simulationState.SoulGhosts.FindWithId(soulGhostId);
            soulGhostBody.SetTransform(soulGhostState.Position, 0);
            soulGhostBody.SetLinearVelocity(soulGhostState.Velocity);
        }

        private void CopySwapFieldToBody(Body swapFieldBody, ushort swapFieldId, MatchSimulationStateS2C simulationState)
        {
            var swapField = simulationState.SwapFields.FindWithId(swapFieldId);
            var swapFieldPosition = simulationState.Players.FindWithId(swapField.PlayerCasterId).Spaceship.Transform.Position;
            swapFieldBody.SetTransform(swapFieldPosition, swapFieldBody.GetAngle());
            var fixture = swapFieldBody.GetFixtureList();
            var shape = (CircleShape) fixture.Shape;
            shape.Radius = swapField.Radius;
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

        private void CopySpikeStateToBody(Body spikeBody, ushort spikeId, FixedClassUnorderedList<EnvironmentSpikeS2C> environmentSpikes)
        {
            var environmentSpike = environmentSpikes.FindWithId(spikeId);
            spikeBody.SetTransform(environmentSpike.Transform.WorldPosition, environmentSpike.Transform.WorldRotationDegrees.ToRadians());
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
        
        private void CopyPlayerHeartStateToBody(Body playerHeartBody, ushort playerId, MatchSimulationStateS2C simulationState)
        {
            var playerState = simulationState.Players.FindWithId(playerId);
            playerHeartBody.SetTransform(playerState.Spaceship.Transform.GetHeartPosition(), 0);
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
                    case PhysicsBodyType.PlayerHeart: CopyPlayerHeartStateToBody(currentBody, bodyData.Id, simulationState); break;
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
        
        private void CopyPlayerHeartStateToBody(Body playerHeartBody, ushort playerId, MatchMakingSimulationStateS2C simulationState)
        {
            var playerState = simulationState.Players.FindWithId(playerId);
            playerHeartBody.SetTransform(playerState.Spaceship.Transform.Position, 0);
            playerHeartBody.SetLinearVelocity(playerState.Spaceship.Transform.Velocity);
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
            world.SetContactFilter(new PlayerCollisionContactFilter(_gamePlayConfigService));
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
            fixtureDef.filter.maskBits = PhysicsCollisionType.Wall.GetCollisionMask();

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
            fixtureDef.filter.maskBits = PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(lavaShape);
        }

        public void AddStageBoundary(ushort id, Vector2[] points, Vector2 position)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.StageBoundary);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var boundaryShape = GetPolygonShape();
            boundaryShape.Set(points);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = boundaryShape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.categoryBits = PhysicsBodyType.StageBoundary.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(boundaryShape);
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
            fixtureDef.filter.maskBits = PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(shape);
        }

        public void AddPlayer(ushort id, ushort teamId, Vector2 position, Vector2 velocity, float radius, float heartRadius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.position = position;
            bodyDef.linearVelocity = velocity;
            bodyDef.type = BodyType.Dynamic;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.PlayerSpaceship);
            bodyDef.allowSleep = false;
            bodyDef.bullet = true;
            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var circleShape = GetCircleShape();
            circleShape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = circleShape;
            fixtureDef.density = 1.0f;
            fixtureDef.friction = 0;
            fixtureDef.filter.categoryBits = PhysicsBodyType.PlayerSpaceship.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.PlayerSpaceship.GetCollisionMask();
            var playerGroupIndex = (short)-teamId;
            fixtureDef.filter.groupIndex = playerGroupIndex;

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(circleShape);
            
            AddPlayerHeart(id, position, heartRadius, playerGroupIndex);
        }

        private void AddPlayerHeart(ushort id, Vector2 position, float heartRadius, short groupIndex)
        {
            var heartBodyDef = GetBodyDef();
            heartBodyDef.position = position;
            heartBodyDef.type = BodyType.Static;
            heartBodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.PlayerHeart);
            heartBodyDef.allowSleep = false;
            var heartBody = _world.CreateBody(heartBodyDef);
            _bodyDefPool.Return(heartBodyDef);

            var heartShape = GetCircleShape();
            heartShape.Radius = heartRadius;

            var heartFixtureDef = GetFixtureDef();
            heartFixtureDef.shape = heartShape;
            heartFixtureDef.isSensor = true;
            heartFixtureDef.filter.categoryBits = PhysicsBodyType.PlayerHeart.GetCollisionsCategory();
            heartFixtureDef.filter.maskBits = PhysicsCollisionType.PlayerHeart.GetCollisionMask();
            heartFixtureDef.filter.groupIndex = groupIndex;

            heartBody.CreateFixture(heartFixtureDef);
            _fixtureDefPool.Return(heartFixtureDef);
            _circleShapePool.Return(heartShape);
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
            fixtureDef.filter.maskBits = PhysicsCollisionType.PlayerBullet.GetCollisionMask();
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
            fixtureDef.filter.maskBits = PhysicsCollisionType.TalentCard.GetCollisionMask();

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
            fixtureDef.filter.maskBits = PhysicsCollisionType.PowerUpBall.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(circleShape);
        }

        // Moles are stationary sensors: they must notice bullets and spaceships without pushing them around.
        public void AddMole(ushort id, Vector2 position, float radius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.Mole);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var circleShape = GetCircleShape();
            circleShape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = circleShape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.categoryBits = PhysicsBodyType.Mole.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.Mole.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(circleShape);
        }

        public void RemoveMole(ushort id)
        {
            var body = GetBody(PhysicsBodyType.Mole, id);
            RemoveBody(body);
        }

        public Body GetBullet(ushort bulletId)
        {
            return GetBody(PhysicsBodyType.PlayerBullet, bulletId);
        }

        public Body GetPowerUpBall(ushort powerUpBallId)
        {
            return GetBody(PhysicsBodyType.PowerUpBall, powerUpBallId);
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

        // Casts the rectangle against two body types at once. firstPriorityBodyType wins when both are inside the shape;
        // ignoreTeamId only applies to players, since they are the only bodies grouped by team. hitBodyData carries the type that was hit.
        public bool RectangleCastByPriority(Vector2 center, Vector2 size, float angleRadians, short ignoreTeamId, PhysicsBodyType firstPriorityBodyType, PhysicsBodyType secondPriorityBodyType, out PhysicsBodyData hitBodyData)
        {
            return TryRectangleCast(center, size, angleRadians, firstPriorityBodyType, secondPriorityBodyType, ignoreTeamId, out hitBodyData);
        }

        private bool TryRectangleCast(Vector2 center, Vector2 size, float angleRadians, PhysicsBodyType firstPriorityBodyType, PhysicsBodyType secondPriorityBodyType, short ignoreTeamId, out PhysicsBodyData hitBodyData)
        {
            _unityMainThreadDispatcher.EnqueueDraw(()=>DebugDrawUtils.DrawRotatedRect(center, size, angleRadians));

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

            var polygonShape = GetPolygonShape();
            polygonShape.SetAsBox(hx, hy);

            var hasCollision = TryShapeCastHit(polygonShape, new Transform(center, rot), aabb, firstPriorityBodyType, secondPriorityBodyType, ignoreTeamId, out hitBodyData);

            _polygonShapePool.Return(polygonShape);

            return hasCollision;
        }

        // Casts the arc against two body types at once. firstPriorityBodyType wins when both are inside the shape;
        // ignoreTeamId only applies to players, since they are the only bodies grouped by team. hitBodyData carries the type that was hit.
        public bool ArcCastByPriority(Vector2 center, float radius, Vector2 direction, float arcAngleDegrees, short ignoreTeamId, PhysicsBodyType firstPriorityBodyType, PhysicsBodyType secondPriorityBodyType, out PhysicsBodyData hitBodyData)
        {
            return TryArcCast(center, radius, direction, arcAngleDegrees, firstPriorityBodyType, secondPriorityBodyType, ignoreTeamId, out hitBodyData);
        }

        private bool TryArcCast(Vector2 center, float radius, Vector2 directon, float arcAngleDegrees, PhysicsBodyType firstPriorityBodyType, PhysicsBodyType secondPriorityBodyType, short ignoreTeamId, out PhysicsBodyData hitBodyData)
        {
            var arcAngleRad = arcAngleDegrees.ToRadians();
            var startAngleRad = directon.ToAngleRadians()-arcAngleRad*0.5f;

            // We use 1 point for the center, leaving up to 7 points for the outer curve
            int vertexCount = 8;
            var vertices = new Vector2[vertexCount];
            vertices[0] = Vector2.Zero; // Center in local space

            for (int i = 0; i < vertexCount - 1; i++)
            {
                // Distribute the remaining points across the arc angle
                float currentAngle = startAngleRad + (arcAngleRad * (i / (float) (vertexCount - 2)));

                vertices[i + 1] = new Vector2(
                    MathF.Cos(currentAngle) * radius,
                    MathF.Sin(currentAngle) * radius
                );
            }

            // 2. Calculate AABB for broadphase query
            // We calculate this in world space to match your RectangleCast logic
            var min = center;
            var max = center;

            foreach (var v in vertices)
            {
                var worldV = v + center;
                min = Vector2.Min(min, worldV);
                max = Vector2.Max(max, worldV);
            }

            _unityMainThreadDispatcher.EnqueueDraw(() => DebugDrawUtils.DrawPolygon(center, vertices));

            var aabb = new AABB(min, max);

            var polygonShape = GetPolygonShape();
            // Set the approximation vertices
            polygonShape.Set(vertices);

            // transformA handles the position. Rotation is baked into vertices
            // but we pass Identity to stay consistent with local-space vertices.
            var hasCollision = TryShapeCastHit(polygonShape, new Transform(center, Matrix3x2.Identity), aabb, firstPriorityBodyType, secondPriorityBodyType, ignoreTeamId, out hitBodyData);

            _polygonShapePool.Return(polygonShape);

            return hasCollision;
        }

        // Runs one broadphase query for both body types. A first-priority hit ends the query at once; a second-priority hit is
        // remembered but the search continues, so a first-priority body that is also inside the shape still wins. ignoreTeamId
        // only filters players, the only bodies grouped by team.
        private bool TryShapeCastHit(PolygonShape castShape, Transform castTransform, AABB aabb, PhysicsBodyType firstPriorityBodyType, PhysicsBodyType secondPriorityBodyType, short ignoreTeamId, out PhysicsBodyData hitBodyData)
        {
            var hasFirstPriorityHit = false;
            var hasSecondPriorityHit = false;
            PhysicsBodyData firstPriorityHitBody = default;
            PhysicsBodyData secondPriorityHitBody = default;

            _world.QueryAABB(fixture =>
            {
                var currentBodyData = (PhysicsBodyData) fixture.Body.UserData;
                var bodyType = currentBodyData.PhysicsBodyType;

                var isFirstPriority = bodyType == firstPriorityBodyType;
                var isSecondPriority = bodyType == secondPriorityBodyType;
                if (!isFirstPriority && !isSecondPriority)
                {
                    return true;
                }

                var isPlayerFromIgnoredTeam = bodyType == PhysicsBodyType.PlayerSpaceship && fixture.FilterData.groupIndex == -ignoreTeamId;
                if (isPlayerFromIgnoredTeam)
                {
                    return true;
                }

                // A second-priority hit is already recorded, so another one cannot change the outcome - only a first-priority hit still matters.
                if (isSecondPriority && hasSecondPriorityHit)
                {
                    return true;
                }

                var input = new ShapeCastInput();
                input.proxyA.Set(castShape, 0);
                input.proxyB.Set(fixture.Shape, 0);
                input.transformA = castTransform;
                input.transformB = fixture.Body.GetTransform();
                input.translationB = Vector2.Zero;

                if (!Contact.ShapeCast(out _, input))
                {
                    return true;
                }

                if (isFirstPriority)
                {
                    hasFirstPriorityHit = true;
                    firstPriorityHitBody = currentBodyData;
                    return false; // nothing outranks a first-priority hit, so stop querying
                }

                hasSecondPriorityHit = true;
                secondPriorityHitBody = currentBodyData;
                return true; // keep looking in case a first-priority body is also inside the shape
            }, aabb);

            if (hasFirstPriorityHit)
            {
                hitBodyData = firstPriorityHitBody;
                return true;
            }

            hitBodyData = secondPriorityHitBody;
            return hasSecondPriorityHit;
        }

        public bool EllipseCastOnPlayers(Vector2 center, float radius, Vector2 direction, float arcAngleDegrees, short ignoreTeamId, out PhysicsBodyData hitBodyData)
        {
            var hasCollision = false;
            hitBodyData = default;

            // Match the arc's footprint: full length along the aim direction is 'radius',
            // and the perpendicular width matches the arc's angular spread at its far edge.
            var halfAngleRad = (arcAngleDegrees * 0.5f).ToRadians();
            var semiMajor = radius * 0.5f;
            var semiMinor = radius * MathF.Sin(halfAngleRad);

            var rot = Matrix3x2.CreateRotation(direction.ToAngleRadians());

            // Ellipse is centered halfway along the aim direction so it spans from the apex to the tip.
            var ellipseCenter = center + direction * semiMajor;

            // Box2D polygons are capped at 8 vertices, so approximate the ellipse with 8 points.
            const int vertexCount = 8;
            var vertices = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                var t = i / (float) vertexCount * MathF.PI * 2f;
                var local = new Vector2(MathF.Cos(t) * semiMajor, MathF.Sin(t) * semiMinor);
                vertices[i] = Vector2.Transform(local, rot);
            }

            var min = ellipseCenter;
            var max = ellipseCenter;

            foreach (var v in vertices)
            {
                var worldV = v + ellipseCenter;
                min = Vector2.Min(min, worldV);
                max = Vector2.Max(max, worldV);
            }

            _unityMainThreadDispatcher.EnqueueDraw(() => DebugDrawUtils.DrawPolygon(ellipseCenter, vertices));

            var aabb = new AABB(min, max);
            PhysicsBodyData hitBody = default;

            _world.QueryAABB(fixture =>
            {
                var currentBodyData = (PhysicsBodyData) fixture.Body.UserData;
                var shouldContinueQuery = true;
                var isPlayerFromNotIgnoredTeam = currentBodyData.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && fixture.FilterData.groupIndex != -ignoreTeamId;
                if (isPlayerFromNotIgnoredTeam)
                {
                    var polygonShape = GetPolygonShape();
                    polygonShape.Set(vertices);

                    var input = new ShapeCastInput();
                    input.proxyA.Set(polygonShape, 0);
                    input.proxyB.Set(fixture.Shape, 0);
                    input.transformA = new Transform(ellipseCenter, Matrix3x2.Identity);
                    input.transformB = fixture.Body.GetTransform();
                    input.translationB = Vector2.Zero;

                    if (Contact.ShapeCast(out _, input))
                    {
                        hasCollision = true;
                        hitBody = currentBodyData;
                    }

                    _polygonShapePool.Return(polygonShape);
                    shouldContinueQuery = !hasCollision;
                }

                return shouldContinueQuery;
            }, aabb);

            hitBodyData = hitBody;

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
            fixtureDef.filter.maskBits = PhysicsCollisionType.StartMatchWall.GetCollisionMask();

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
            fixtureDef.filter.maskBits = PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(shape);
        }

        public void AddEnvironmentSpike(ushort id, Vector2 position, float rotationDegrees, Vector2 size)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.angle = rotationDegrees.ToRadians();
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.EnvironmentSpike);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetPolygonShape();
            shape.SetAsBox(size.X * 0.5f, size.Y * 0.5f);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;

            fixtureDef.filter.categoryBits = PhysicsBodyType.EnvironmentSpike.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();

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
            fixtureDef.filter.maskBits = PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(shape);
        }

        public void AddKOProjectile(ushort id, ushort teamId, Vector2 position, float radius, Vector2 velocity)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.KOProjectile);
            bodyDef.bullet = true;
            bodyDef.linearVelocity = velocity;
            
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
            fixtureDef.filter.maskBits = PhysicsCollisionType.KOProjectile.GetCollisionMask();

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

        public void AddFrigidBlock(ushort id, Vector2 position, Vector2 rotation, Vector2 size, Vector2 velocity, float density, float restitution, float linearDamping, float angularDamping)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.angle = rotation.ToAngleRadians();
            bodyDef.linearVelocity = velocity;
            bodyDef.linearDamping = linearDamping;
            bodyDef.angularDamping = angularDamping;
            bodyDef.bullet = true;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.FrigidBlock);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var boxShape = GetPolygonShape();
            boxShape.SetAsBox(size.X * 0.5f, size.Y * 0.5f);

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = boxShape;
            fixtureDef.density = density;
            fixtureDef.friction = 0;
            fixtureDef.restitution = restitution;
            fixtureDef.filter.categoryBits = PhysicsBodyType.FrigidBlock.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.FrigidBlock.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _polygonShapePool.Return(boxShape);
        }

        public Body GetFrigidBlock(ushort id)
        {
            return GetBody(PhysicsBodyType.FrigidBlock, id);
        }

        public void RemoveFrigidBlock(ushort id)
        {
            var body = GetBody(PhysicsBodyType.FrigidBlock, id);
            RemoveBody(body);
        }

        public void AddGrapplingHookProjectile(ushort id, ushort teamId, Vector2 position, float radius, Vector2 velocity)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.GrapplingHookProjectile);
            bodyDef.bullet = true;
            bodyDef.linearVelocity = velocity;

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetCircleShape();
            shape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0.3f;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.categoryBits = PhysicsBodyType.GrapplingHookProjectile.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.GrapplingHookProjectile.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(shape);
        }

        public void UpdateGrapplingHookProjectile(ushort id, Vector2 position, Vector2 velocity)
        {
            var body = GetBody(PhysicsBodyType.GrapplingHookProjectile, id);
            body.SetTransform(position, 0);
            body.SetLinearVelocity(velocity);
        }

        public void RemoveGrapplingHookProjectile(ushort id)
        {
            var body = GetBody(PhysicsBodyType.GrapplingHookProjectile, id);
            RemoveBody(body);
        }

        public void AddFishingRodTip(ushort id, ushort teamId, Vector2 position, float radius, Vector2 velocity)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.FishingRodTip);
            bodyDef.bullet = true;
            bodyDef.linearVelocity = velocity;

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetCircleShape();
            shape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0.3f;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.categoryBits = PhysicsBodyType.FishingRodTip.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.FishingRodTip.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(shape);
        }

        public void UpdateFishingRodTip(ushort id, Vector2 position, Vector2 velocity)
        {
            var body = GetBody(PhysicsBodyType.FishingRodTip, id);
            body.SetTransform(position, 0);
            body.SetLinearVelocity(velocity);
        }

        public void RemoveFishingRodTip(ushort id)
        {
            var body = GetBody(PhysicsBodyType.FishingRodTip, id);
            RemoveBody(body);
        }

        public void AddSoulGhost(ushort id, ushort teamId, Vector2 position, float radius, Vector2 velocity)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Dynamic;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(id, PhysicsBodyType.SoulGhost);
            bodyDef.bullet = true;
            bodyDef.linearVelocity = velocity;

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetCircleShape();
            shape.Radius = radius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0.3f;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.categoryBits = PhysicsBodyType.SoulGhost.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.SoulGhost.GetCollisionMask();

            body.CreateFixture(fixtureDef);
            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(shape);
        }

        public void RemoveSoulGhost(ushort id)
        {
            var body = GetBody(PhysicsBodyType.SoulGhost, id);
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
            fixtureDef.filter.maskBits = PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();

            body.CreateFixture(fixtureDef);

            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(shape);
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

        public void EnablePlayerToCollideWithPlayers(ushort playerId)
        {
            var body = GetBody(PhysicsBodyType.PlayerSpaceship, playerId);
            var fixture = body.GetFixtureList();
            var filter = fixture.FilterData;
            // Add CollideOnlyWithPlayer to maskBits so caster.mask & enemy.category != 0
            filter.maskBits |= PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();
            // Add PlayerSpaceship collision bit to categoryBits so enemy.mask & caster.category != 0
            filter.categoryBits |= PhysicsCollisionType.PlayerSpaceship.GetCollisionMask();
            fixture.FilterData = filter;
            body.SetAwake(true);
        }

        public void DisablePlayerToCollideWithPlayers(ushort playerId)
        {
            var body = GetBody(PhysicsBodyType.PlayerSpaceship, playerId);
            var fixture = body.GetFixtureList();
            var filter = fixture.FilterData;
            filter.maskBits = PhysicsCollisionType.PlayerSpaceship.GetCollisionMask();
            filter.categoryBits = PhysicsBodyType.PlayerSpaceship.GetCollisionsCategory();
            fixture.FilterData = filter;
            body.SetAwake(true);
        }
        
        public void EnableRockBody(ushort playerId, float radiusMultiplier, float density, float restitution)
        {
            var body = GetBody(PhysicsBodyType.PlayerSpaceship, playerId);
            var fixture = body.GetFixtureList();

            var circleShape = (CircleShape) fixture.Shape;
            circleShape.Radius *= radiusMultiplier;

            var filter = fixture.FilterData;
            filter.groupIndex = 0; // clear team grouping so it collides with teammates too; category/mask below decide the rest
            filter.categoryBits |= PhysicsCollisionType.PlayerSpaceship.GetCollisionMask();
            filter.categoryBits |= PhysicsCollisionType.Wall.GetCollisionMask();
            filter.categoryBits |= PhysicsCollisionType.PowerUpBall.GetCollisionMask();
            // Expose the GrapplingHookProjectile category so a hook (mask = GrapplingHookProjectile) can attach to a rock.
            filter.categoryBits |= PhysicsCollisionType.GrapplingHookProjectile.GetCollisionMask();
            // Expose the SoulGhost category so a ghost (mask = SoulGhost) is stopped by a rock instead of flying through it.
            filter.categoryBits |= PhysicsCollisionType.SoulGhost.GetCollisionMask();
            filter.maskBits |= PhysicsCollisionType.AnyObjectThatCollidesOnlyWithPlayer.GetCollisionMask();
            filter.maskBits |= PhysicsCollisionType.Wall.GetCollisionMask();
            filter.maskBits |= PhysicsCollisionType.GrapplingHookProjectile.GetCollisionMask();
            fixture.FilterData = filter;

            fixture.Density = density;
            fixture.Restitution = restitution;
            body.ResetMassData();
            body.SetAwake(true);
        }

        public void DisableRockBody(ushort playerId, float baseRadius, ushort teamId)
        {
            var body = GetBody(PhysicsBodyType.PlayerSpaceship, playerId);
            var fixture = body.GetFixtureList();

            var circleShape = (CircleShape) fixture.Shape;
            circleShape.Radius = baseRadius;

            var filter = fixture.FilterData;
            filter.categoryBits = PhysicsBodyType.PlayerSpaceship.GetCollisionsCategory();
            filter.maskBits = PhysicsCollisionType.PlayerSpaceship.GetCollisionMask();
            filter.groupIndex = (short)-teamId;
            fixture.FilterData = filter;

            fixture.Density = DEFAULT_PLAYER_DENSITY;
            fixture.Restitution = DEFAULT_PLAYER_RESTITUTION;
            body.ResetMassData();
            body.SetAwake(true);
        }

        public void EnablePlayerHeartCollider(ushort playerId)
        {
            var body = GetBody(PhysicsBodyType.PlayerHeart, playerId);
            var fixture = body.GetFixtureList();
            var filter = fixture.FilterData;
            filter.maskBits = PhysicsCollisionType.PlayerHeart.GetCollisionMask();
            fixture.FilterData = filter;
            body.SetAwake(true);
        }

        public void DisablePlayerHeartCollider(ushort playerId)
        {
            var body = GetBody(PhysicsBodyType.PlayerHeart, playerId);
            var fixture = body.GetFixtureList();
            var filter = fixture.FilterData;
            filter.maskBits = 0x0000;
            fixture.FilterData = filter;
            body.SetAwake(true);
        }

        public void AddChickenEgg(ushort eggId, ushort teamId, Vector2 position, float eggRadius)
        {
            var bodyDef = GetBodyDef();
            bodyDef.type = BodyType.Static;
            bodyDef.position = position;
            bodyDef.userData = new PhysicsBodyData(eggId, PhysicsBodyType.ChickenEgg);

            var body = _world.CreateBody(bodyDef);
            _bodyDefPool.Return(bodyDef);

            var shape = GetCircleShape();
            shape.Radius = eggRadius;

            var fixtureDef = GetFixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 0;
            fixtureDef.friction = 0;
            fixtureDef.isSensor = true;
            fixtureDef.filter.groupIndex = (short)-teamId;

            fixtureDef.filter.categoryBits = PhysicsBodyType.ChickenEgg.GetCollisionsCategory();
            fixtureDef.filter.maskBits = PhysicsCollisionType.ChickenEgg.GetCollisionMask();

            body.CreateFixture(fixtureDef);

            _fixtureDefPool.Return(fixtureDef);
            _circleShapePool.Return(shape);
        }

        public Body GetKOProjectile(ushort koProjectileId)
        {
            return GetBody(PhysicsBodyType.KOProjectile, koProjectileId);
        }

        public Body GetGrapplingHookProjectile(ushort grapplingHookProjectileId)
        {
            return GetBody(PhysicsBodyType.GrapplingHookProjectile, grapplingHookProjectileId);
        }

        public Body GetSoulGhost(ushort soulGhostId)
        {
            return GetBody(PhysicsBodyType.SoulGhost, soulGhostId);
        }

        public Body GetFishingRodTip(ushort fishingRodTipId)
        {
            return GetBody(PhysicsBodyType.FishingRodTip, fishingRodTipId);
        }
        
        public Body GetChickenEgg(ushort chieckEggId)
        {
            return GetBody(PhysicsBodyType.ChickenEgg, chieckEggId);
        }

        public void RemoveChickenEgg(ushort eggId)
        {
            var body = GetBody(PhysicsBodyType.ChickenEgg, eggId);
            RemoveBody(body);
        }

        public bool RayCast(Vector2 originPoint, Vector2 endPoint, out PhysicsBodyData hitBodyData, PhysicsBodyType[] bodyTypesRayCastCanHit = null, PhysicsBodyData? ignoredBody = null)
        {
            // Box2D's RayCast ignores any fixture whose interior already contains the ray origin. Detect that case
            // explicitly and treat it as the closest possible hit (collision point == ray origin, fraction 0).
            var isOriginPointInsideABody = TryGetBodyContainingPoint(originPoint, bodyTypesRayCastCanHit, ignoredBody, out var bodyHitData);
            var didHit = false;

            if (isOriginPointInsideABody)
            {
                didHit = true;
            }
            else
            {
                var closestFraction = 1f;
                _world.RayCast(OnRayCastHit, originPoint, endPoint);

                void OnRayCastHit(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
                {
                    var body = fixture.Body;
                    var bodyData = (PhysicsBodyData) body.UserData;

                    var didRayHitClosetBody = fraction <= closestFraction && !IsIgnoredBody(bodyData, ignoredBody) &&
                                              CanRayCastHit(bodyTypesRayCastCanHit, bodyData.PhysicsBodyType);

                    if (didRayHitClosetBody)
                    {
                        didHit = true;
                        closestFraction = fraction;
                        bodyHitData = bodyData;
                    }
                }
            }

            _unityMainThreadDispatcher.EnqueueDraw(() => DebugDrawUtils.DrawLine(originPoint.ToUnityVector2(), endPoint.ToUnityVector2(), didHit ? UnityEngine.Color.green : UnityEngine.Color.red));

            hitBodyData = bodyHitData;
            return didHit;
        }

        private bool TryGetBodyContainingPoint(Vector2 point, PhysicsBodyType[] bodyTypesRayCastCanHit, PhysicsBodyData? ignoredBody, out PhysicsBodyData hitBodyData)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData) currentBody.UserData;

                if (!IsIgnoredBody(bodyData, ignoredBody) && CanRayCastHit(bodyTypesRayCastCanHit, bodyData.PhysicsBodyType))
                {
                    var fixture = currentBody.GetFixtureList();

                    while (fixture != null)
                    {
                        if (fixture.TestPoint(point))
                        {
                            hitBodyData = bodyData;
                            return true;
                        }

                        fixture = fixture.GetNext();
                    }
                }

                currentBody = currentBody.GetNext();
            }

            hitBodyData = default;
            return false;
        }

        private static bool IsIgnoredBody(PhysicsBodyData bodyData, PhysicsBodyData? ignoredBody)
        {
            return ignoredBody.HasValue && ignoredBody.Value.Id == bodyData.Id && ignoredBody.Value.PhysicsBodyType == bodyData.PhysicsBodyType;
        }

        // Allocation-free replacement for the LINQ Array.Contains used in the per-raycast hot path.
        private static bool CanRayCastHit(PhysicsBodyType[] bodyTypesRayCastCanHit, PhysicsBodyType bodyType)
        {
            return bodyTypesRayCastCanHit == null || System.Array.IndexOf(bodyTypesRayCastCanHit, bodyType) >= 0;
        }
    }
}
