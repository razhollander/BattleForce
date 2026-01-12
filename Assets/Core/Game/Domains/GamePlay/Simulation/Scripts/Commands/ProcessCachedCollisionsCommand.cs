using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Talent;
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
            Body bulletBody;
            PlayerBulletS2C bulletModel;
            if (isPlayerToBulletCollision)
            {
                if (!_matchDataService.SimulationState.TryGetBulletById(objectB.Id, out bulletModel))
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);
                    return;
                }
                playerId = objectA.Id;
                bulletBody = contact.FixtureB.Body;
            }
            else
            {
                if (!_matchDataService.SimulationState.TryGetBulletById(objectA.Id, out bulletModel))
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);
                    return;
                }
                
                playerId = objectB.Id;
                bulletBody = contact.FixtureA.Body;
            }

            DestroyBullet(bulletModel, bulletBody);
            _playerHitCommand
                .SetPlayerId(playerId)
                .SetHitDamage(_gamePlayConfig.PlayerBullet.HitDamage)
                .SetProcessedTick(_processedTick)
                .Execute();
        }

        private void DestroyBullet(PlayerBulletS2C bulletModel, Body bulletBody)
        {
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

            if (!TryGetTalentCardToBulletCollisionData(objectA, objectB, contact, isBulletToCardCollision, out var bulletModel, out int talentCardIndex, out var bulletBody, out var cardBody))
            {
                return;
            }

            DestroyBullet(bulletModel, bulletBody);
            ref var talentCard = ref _matchDataService.SimulationState.TalentCards.GetByIndex(talentCardIndex);
            talentCard.Health -= _gamePlayConfig.PlayerBullet.HitDamage;
            if (talentCard.Health > 0)
            {
                return;
            }
            
            DestroyTalentCard(talentCard, cardBody);
        }

        private bool TryGetTalentCardToBulletCollisionData(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact, bool isBulletToCardCollision, out PlayerBulletS2C bulletModel,
            out int talentCardIndex , out Body bulletBody, out Body cardBody)
        {
            ushort cardId;
            talentCardIndex = default;
            cardBody = default;
            bulletBody = default;
            if (isBulletToCardCollision)
            {
                if(_matchDataService.SimulationState.TryGetBulletById(objectA.Id, out bulletModel))
                {
                    bulletBody = contact.FixtureA.Body;
                    cardId = objectB.Id;
                    cardBody = contact.FixtureB.Body;
                }
                else
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);
                    return false;
                }
            }
            else
            {
                if(_matchDataService.SimulationState.TryGetBulletById(objectB.Id, out bulletModel))
                {
                    bulletBody = contact.FixtureB.Body;
                    cardId = objectA.Id;
                    cardBody = contact.FixtureA.Body;
                }
                else
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);
                    return false;
                }
            }

            if (_matchDataService.SimulationState.TryGetTalentCardIndexById(cardId, out int index))
            {
                talentCardIndex = index;
                return true;
            }

            LogService.LogTopic("Card was already destroyed in this frame!", LogTopicType.ServerPhysics);
            return false;
        }

        private void DestroyTalentCard(TalentCardS2C card, Body cardBody)
        {
            _matchDataService.SimulationState.RemoveTalentCardById(card.Id);
            _physicsSimulator.RemoveBody(cardBody);
            _matchNetEventsDataService.AddTalentCardObtainedNetEvent(_processedTick, card.Id);
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