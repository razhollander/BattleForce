using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.Commands
{
    public class MatchMakingProcessCachedCollisionsCommand : BaseCommand, ICommandVoid
    {
        private IPhysicsSimulator _physicsSimulator;
        private IMatchMakingDataService _matchMakingDataService;
        private IMatchNetEventsDataService _matchNetEventsDataService;

        private int _processedTick;

        public MatchMakingProcessCachedCollisionsCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _matchNetEventsDataService = _diContainer.Resolve<IMatchNetEventsDataService>();
        }

        public void Execute()
        {
            ProcessCollisions();
        }

        private void ProcessCollisions()
        {
            var cachedCollisions = _physicsSimulator.GetCachedCollisions();

            for (int i = 0; i < cachedCollisions.Count; i++) // this must stay for and not forearch, since if we destroy and object an event 'ContactEnd' will be added
            {
                var collisionEvent = cachedCollisions[i];

                var objectA = collisionEvent.BodyDataA;
                var objectB = collisionEvent.BodyDataB;

                if (collisionEvent.Type != PhysicsEventEventType.Begin)
                {
                    continue;
                }

                HandlePlayerWallCollision(objectA, objectB, collisionEvent.Contact);
                HandleBulletWallCollision(objectA, objectB, collisionEvent.Contact);
            }

            _physicsSimulator.ClearCachedCollisions();
        }

        private void HandleBulletWallCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact)
        {
            var isBulletToWallCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerBullet && objectB.PhysicsBodyType == PhysicsBodyType.Wall;
            var isWallToBulletCollision = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.PlayerBullet;
            var isCollision = isWallToBulletCollision || isBulletToWallCollision;

            if (!isCollision)
            {
                return;
            }

            Body bulletBody;
            PlayerBulletS2C bulletModel;

            if (isWallToBulletCollision)
            {
                if (!_matchMakingDataService.SimulationState.TryGetBulletById(objectB.Id, out bulletModel))
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);

                    return;
                }

                bulletBody = contact.FixtureB.Body;
            }
            else
            {
                if (!_matchMakingDataService.SimulationState.TryGetBulletById(objectA.Id, out bulletModel))
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);

                    return;
                }

                bulletBody = contact.FixtureA.Body;
            }

            DestroyBullet(bulletModel, bulletBody);
        }

        private void DestroyBullet(PlayerBulletS2C bulletModel, Body bulletBody)
        {
            _matchMakingDataService.SimulationState.RemoveBulletById(bulletModel.Id);
            _physicsSimulator.RemoveBody(bulletBody);
            LogService.LogError($"Bullet destroyed! {bulletModel.Id}");
            _matchNetEventsDataService.AddBulletDestroyedNetEvent(_processedTick, bulletModel.Id, bulletModel.Position);
        }

        private void HandlePlayerWallCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact)
        {
            bool isPlayerToWallCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.Wall;
            bool isWallToPlayerCollision = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;
            var isCollision = isPlayerToWallCollision || isWallToPlayerCollision;

            if (!isCollision)
            {
                return;
            }

            var playerModel = GetPlayerFromCollision(objectA, objectB, isPlayerToWallCollision, isWallToPlayerCollision);
            var relativeVelocity = playerModel.Spaceship.Transform.Velocity;
            contact.GetWorldManifold(out var worldManifold);
            var collisionNormal = worldManifold.normal;
            var reflectedVelocity = relativeVelocity.ReflectFromWall(collisionNormal);
            playerModel.Spaceship.Transform.Velocity = reflectedVelocity;

            LogService.LogTopic($"new pos {_physicsSimulator.GetPlayer(playerModel.Id).Position}, prev pos: {playerModel.Spaceship.Transform.Position} ",
                LogTopicType.ServerNetwork);

            playerModel.Spaceship.Transform.Direction = reflectedVelocity.Length() > 0
                ? System.Numerics.Vector2.Normalize(reflectedVelocity)
                : System.Numerics.Vector2.Zero;
        }

        private MatchMakingPlayerStateS2C GetPlayerFromCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, bool isPlayerToWallCollision, bool isWallToPlayerCollision)
        {
            if (isPlayerToWallCollision)
            {
                return _matchMakingDataService.SimulationState.GetPlayerById(objectA.Id);
            }

            if (isWallToPlayerCollision)
            {
                return _matchMakingDataService.SimulationState.GetPlayerById(objectB.Id);
            }

            throw new System.Exception("No collision!");
        }
    }
}