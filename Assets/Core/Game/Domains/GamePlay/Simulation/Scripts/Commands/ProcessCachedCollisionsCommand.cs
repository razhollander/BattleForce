using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class ProcessCachedCollisionsCommand : BaseCommand, ICommandVoid
    {
        private IPhysicsSimulator _physicsSimulator;
        private IMatchDataService _matchDataService;
        private ICommandFactory _commandFactory;

        public override void ResolveDependencies()
        {
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
        }

        public void Execute()
        {
            ProcessCollisions();
        }
        
        private void ProcessCollisions()
        {
            var cachedCollisions = _physicsSimulator.GetCachedCollisions();

            foreach (var collisionEvent in cachedCollisions)
            {
                if (collisionEvent.Type != EventType.Begin)
                {
                    continue;
                }
                var objectA = (PhysicsBodyData) collisionEvent.FixtureA.Body.UserData;
                var objectB = (PhysicsBodyData) collisionEvent.FixtureB.Body.UserData;
                HandlePlayerWallCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletCollision(objectA, objectB, collisionEvent.Contact);
            }

            _physicsSimulator.ClearCachedCollisions();
        }

        private void HandlePlayerBulletCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact)
        {
            var isBulletToPlayerCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerBullet && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;
            var isPlayerToBulletCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.PlayerBullet;
            var isCollision = isPlayerToBulletCollision || isBulletToPlayerCollision;
            if (!isCollision)
            {
                return;
            }
            
            PlayerStateS2C playerModel = default;
            PlayerBulletS2C bulletModel = default;

            if (isPlayerToBulletCollision)
            {
                playerModel = _matchDataService.GetPlayer(objectA.Id);
                bulletModel = _matchDataService.GetBullet(objectB.Id);
            }
            else if (isBulletToPlayerCollision)
            {
                playerModel = _matchDataService.GetPlayer(objectB.Id);
                bulletModel = _matchDataService.GetBullet(objectA.Id);
            }
            
            _commandFactory.CreateCommandVoid<PlayerHitCommand>()
            playerModel.Health -= 1;
            _matchDataService.SetPlayer(playerData.Id, playerModel);

            _matchDataService.RemoveBullet(bulletData.Id);
            _physicsSimulator.RemoveBody(bulletData.Id);

            var playerHitByBulletEvent = new PlayerHitByBulletEvent(playerModel.Id, bulletData.Id, contact);
            _eventSystem.Dispatch(playerHitByBulletEvent);
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
            
            PlayerStateS2C playerModel = default;
            Body playerBody = default;

            if (isPlayerToWallCollision)
            {
                playerModel = _matchDataService.GetPlayer(objectA.Id);
            }
            else if (isWallToPlayerCollision)
            {
                playerModel = _matchDataService.GetPlayer(objectB.Id);
            }

            

            var relativeVelocity = playerModel.Spaceship.Transform.Velocity;
            contact.GetWorldManifold(out var worldManifold);
            var collisionNormal = worldManifold.normal;
            var reflectedVelocity = relativeVelocity.ReflectFromWall(collisionNormal);
            playerModel.Spaceship.Transform.Velocity = reflectedVelocity;
            //Debug.Log($"new pos {_physicsSimulator.GetPlayer(playerModel.Id).Position}, prev pos: {playerModel.Spaceship.Transform.Position} ");
            playerModel.Spaceship.Transform.Direction = reflectedVelocity.Length() > 0
                ? System.Numerics.Vector2.Normalize(reflectedVelocity)
                : System.Numerics.Vector2.Zero;

            Debug.Log("Collision!");
            _matchDataService.SetPlayer(playerModel.Id, playerModel);
        }
    }
}