using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class ProcessCachedCollisionsCommand : BaseCommand, ICommandVoid
    {
        private IPhysicsSimulator _physicsSimulator;
        private IMatchDataService _matchDataService;
        private ICommandFactory _commandFactory;
        private SimulationGamePlayConfig _gamePlayConfig;
        private IMatchNetEventsDataService _matchNetEventsDataService;
        private IPlayersTalentsManager _playersTalentsManager;
        
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
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
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
                if (collisionEvent.Type != PhysicsEventEventType.Begin)
                {
                    continue;
                }

                var objectA = collisionEvent.BodyDataA;
                var objectB = collisionEvent.BodyDataB;
                HandlePlayerWallCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletTalentCardCollision(objectA, objectB, collisionEvent.Contact);
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
            
            ushort playerId;
            PlayerBulletS2C bulletModel;
            Body bulletBody;

            if (isPlayerToBulletCollision)
            {
                playerId = objectA.Id;
                bulletModel = _matchDataService.SimulationState.GetBulletById(objectB.Id);
                bulletBody = contact.FixtureB.Body;
            }
            else
            {
                playerId = objectB.Id;
                bulletModel = _matchDataService.SimulationState.GetBulletById(objectA.Id);
                bulletBody = contact.FixtureA.Body;
            }

            _playerHitCommand
                .SetPlayerId(playerId)
                .SetHitDamage(_gamePlayConfig.PlayerBullet.HitDamage)
                .SetProcessedTick(_processedTick)
                .Execute();
            
            _matchDataService.SimulationState.RemoveBulletById(bulletModel.Id);
            _physicsSimulator.RemoveBody(bulletBody);
            LogService.LogError($"Bullet destroyed! {bulletModel.Id}");
            _matchNetEventsDataService.AddBulletDestroyedNetEvent(_processedTick, bulletModel.Id, bulletModel.Position);
        }

        private void HandlePlayerBulletTalentCardCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact)
        {
            var isBulletToCardCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerBullet && objectB.PhysicsBodyType == PhysicsBodyType.TalentCard;
            var isCardToBulletCollision = objectA.PhysicsBodyType == PhysicsBodyType.TalentCard && objectB.PhysicsBodyType == PhysicsBodyType.PlayerBullet;
            var isCollision = isBulletToCardCollision || isCardToBulletCollision;

            if (!isCollision)
            {
                return;
            }

            PlayerBulletS2C bulletModel;
            ushort cardId;
            Body bulletBody;
            Body cardBody;

            if (isBulletToCardCollision)
            {
                bulletModel = _matchDataService.SimulationState.GetBulletById(objectA.Id);
                bulletBody = contact.FixtureA.Body;
                cardId = objectB.Id;
                cardBody = contact.FixtureB.Body;
            }
            else
            {
                bulletModel = _matchDataService.SimulationState.GetBulletById(objectB.Id);
                bulletBody = contact.FixtureB.Body;
                cardId = objectA.Id;
                cardBody = contact.FixtureA.Body;
            }

            // Destroy Bullet
            _matchDataService.SimulationState.RemoveBulletById(bulletModel.Id);
            _physicsSimulator.RemoveBody(bulletBody);
            _matchNetEventsDataService.AddBulletDestroyedNetEvent(_processedTick, bulletModel.Id, bulletModel.Position);

            // Damage Card
            ref var card = ref _matchDataService.SimulationState.GetTalentCardById(cardId);
            card.Health -= _gamePlayConfig.PlayerBullet.HitDamage;

            if (card.Health <= 0)
            {
                // Obtain Talent
                _playersTalentsManager.TryAddTalentToPlayer(card.TalentType, bulletModel.BelongToPlayerId);

                // Destroy Card
                _matchDataService.SimulationState.RemoveTalentCardById(card.Id);
                _physicsSimulator.RemoveBody(cardBody);
                _matchNetEventsDataService.AddTalentCardDestroyedNetEvent(_processedTick, card.Id);
            }
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
            LogService.LogTopic($"new pos {_physicsSimulator.GetPlayer(playerModel.Id).Position}, prev pos: {playerModel.Spaceship.Transform.Position} ", LogTopicType.ServerNetwork);
            playerModel.Spaceship.Transform.Direction = reflectedVelocity.Length() > 0
                ? System.Numerics.Vector2.Normalize(reflectedVelocity)
                : System.Numerics.Vector2.Zero;
        }

        private PlayerStateS2C GetPlayerFromCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, bool isPlayerToWallCollision, bool isWallToPlayerCollision)
        {
            if (isPlayerToWallCollision)
            {
                return _matchDataService.SimulationState.GetPlayerById(objectA.Id);
            }
            if (isWallToPlayerCollision)
            {
                return _matchDataService.SimulationState.GetPlayerById(objectB.Id);
            }
            
            throw new System.Exception("No collision!");
        }
    }
}