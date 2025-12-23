using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class ProcessCachedCollisionsCommand : BaseCommand, ICommandVoid
    {
        private IPhysicsSimulator _physicsSimulator;
        private IMatchDataService _matchDataService;
        private ICommandFactory _commandFactory;
        private SimulationGamePlayConfig _gamePlayConfig;
        private IMatchNetEventsDataService _matchNetEventsDataService;
        
        private int _processedTick;
        private PlayerHitCommand _playerHitCommand;

        public ProcessCachedCollisionsCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _playerHitCommand = _commandFactory.CreateCommandVoid<PlayerHitCommand>();
            _matchNetEventsDataService = _diContainer.Resolve<IMatchNetEventsDataService>();
        }

        public void Execute()
        {
            ProcessCollisions();
        }

        private void ProcessCollisions()
        {
            var cachedCollisions = _physicsSimulator.GetCachedCollisions();

            for (int i = 0; i < cachedCollisions.Count; i++) // this must stay for, since if we destroy and object an event 'ContactEnd' will be added
            {
                var collisionEvent = cachedCollisions[i];
                if (collisionEvent.Type != PhysicsEventEventType.Begin)
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
            
            PlayerStateS2C playerModel;
            PlayerBulletS2C bulletModel;
            Body bulletBody;

            if (isPlayerToBulletCollision)
            {
                playerModel = _matchDataService.GetPlayer(objectA.Id);
                bulletModel = _matchDataService.GetBullet(objectB.Id);
                bulletBody = contact.FixtureB.Body;
            }
            else
            {
                playerModel = _matchDataService.GetPlayer(objectB.Id);
                bulletModel = _matchDataService.GetBullet(objectA.Id);
                bulletBody = contact.FixtureA.Body;
            }

            _playerHitCommand
                .SetPlayerId(playerModel.Id)
                .SetHitDamage(_gamePlayConfig.PlayerBullet.HitDamage)
                .SetProcessedTick(_processedTick)
                .Execute();
            
            _matchDataService.RemoveBullet(bulletModel.Id);
            _physicsSimulator.RemoveBody(bulletBody);
            Debug.Log($"Bullet destroyed! {bulletModel.Id}");
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

            _matchDataService.SetPlayer(playerModel.Id, playerModel);
        }
    }
}