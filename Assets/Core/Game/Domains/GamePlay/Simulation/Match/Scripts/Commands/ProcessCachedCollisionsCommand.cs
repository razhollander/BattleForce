using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class ProcessCachedCollisionsCommand : BaseCommand, ICommandVoid
    {
        private IPhysicsSimulator _physicsSimulator;
        private IMatchDataService _matchDataService;
        private ICommandFactory _commandFactory;
        private SimulationGamePlayConfig _gamePlayConfig;
        private INetEventsDataService _netEventsDataService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        
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
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
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

                HandlePlayerLavaCollision(objectA, objectB, collisionEvent.Type);

                if (collisionEvent.Type != PhysicsEventEventType.Begin)
                {
                    continue;
                }

                HandlePlayerWallCollision(objectA, objectB, collisionEvent.Contact);
                HandlePowerUpBallWallCollision(objectA, objectB, collisionEvent.Contact);
                HandleBulletWallCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletTalentCardCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletPowerUpCollision(objectA, objectB, collisionEvent.Contact);
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
                if (!_matchDataService.SimulationState.TryGetBulletById(objectB.Id, out bulletModel))
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);
                    return;
                }
                bulletBody = contact.FixtureB.Body;
            }
            else
            {
                if (!_matchDataService.SimulationState.TryGetBulletById(objectA.Id, out bulletModel))
                {
                    LogService.LogTopic("Bullet was already destroyed in this frame!", LogTopicType.ServerPhysics);
                    return;
                }
                
                bulletBody = contact.FixtureA.Body;
            }

            DestroyBullet(bulletModel, bulletBody);
        }

        private void HandlePlayerLavaCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, PhysicsEventEventType eventType)
        {
            var isPlayerLava = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.Lava;
            var isLavaPlayer = objectA.PhysicsBodyType == PhysicsBodyType.Lava && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerLava && !isLavaPlayer)
            {
                return;
            }

            var playerId = isPlayerLava ? objectA.Id : objectB.Id;

            if (eventType == PhysicsEventEventType.Begin)
            {
                _playersInLavaTrackerService.OnPlayerEnterLava(playerId);
            }
            else if (eventType == PhysicsEventEventType.End)
            {
                _playersInLavaTrackerService.OnPlayerExitLava(playerId);
            }
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
            _netEventsDataService.AddBulletDestroyedNetEvent(_processedTick, bulletModel.Id, bulletModel.Position);
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
            var isTalentCardAlive = talentCard.Health > 0;

            if (isTalentCardAlive)
            {
                _netEventsDataService.AddTalentCardHitNetEvent(_processedTick, talentCard.Id, talentCard.Health);
            }
            else
            {
                _netEventsDataService.AddTalentCardObtainedNetEvent(_processedTick, talentCard.Id, bulletModel.BelongToPlayerId);
                DestroyTalentCard(talentCard, cardBody);
            }
        }

        private bool TryGetTalentCardToBulletCollisionData(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact, bool isBulletToCardCollision, out PlayerBulletS2C bulletModel,
            out int talentCardIndex , out Body bulletBody, out Body cardBody)
        {
            ushort cardId;
            ushort bulletId; // dont delete used in logs
            talentCardIndex = default;
            cardBody = default;
            bulletBody = default;

            if (isBulletToCardCollision)
            {
                bulletId = objectA.Id;
                bulletBody = contact.FixtureA.Body;
                cardId = objectB.Id;
                cardBody = contact.FixtureB.Body;
            }
            else
            {
                bulletId = objectB.Id;
                bulletBody = contact.FixtureB.Body;
                cardId = objectA.Id;
                cardBody = contact.FixtureA.Body;
            }

            if(!_matchDataService.SimulationState.TryGetBulletById(bulletId, out bulletModel))
            {
                LogService.LogTopic($"Bullet {bulletId} was already destroyed in this frame!", LogTopicType.ServerPhysics);
                return false;
            }

            if (!_matchDataService.SimulationState.TryGetTalentCardIndexById(cardId, out talentCardIndex))
            {
                LogService.LogTopic("Card was already destroyed in this frame!", LogTopicType.ServerPhysics);
                return false;
            }
            
            return true;
        }

        private void DestroyTalentCard(TalentCardS2C card, Body cardBody)
        {
            _matchDataService.SimulationState.RemoveTalentCardById(card.Id);
            _physicsSimulator.RemoveBody(cardBody);
        }

        private void HandlePowerUpBallWallCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact)
        {
            bool isPowerUpBallToWallCollision = objectA.PhysicsBodyType == PhysicsBodyType.PowerUpBall && objectB.PhysicsBodyType == PhysicsBodyType.Wall;
            bool isWallToPowerUpBallCollision = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.PowerUpBall;
            var isCollision = isPowerUpBallToWallCollision || isWallToPowerUpBallCollision;
            if (!isCollision)
            {
                return;
            }
            
            ref var powerUpBallModel = ref _matchDataService.SimulationState.GetPowerUpBallById(isPowerUpBallToWallCollision ? objectA.Id : objectB.Id);
            var relativeVelocity = powerUpBallModel.Velocity;
            contact.GetWorldManifold(out var worldManifold);
            var collisionNormal = worldManifold.normal;
            var reflectedVelocity = relativeVelocity.ReflectFromWall(collisionNormal);
            powerUpBallModel.Velocity = reflectedVelocity;
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
            if (!relativeVelocity.IsFacingWall(collisionNormal))
            {
                return;
            }
            
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

        private void HandlePlayerBulletPowerUpCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact)
        {
            var isBulletToPowerUp = objectA.PhysicsBodyType == PhysicsBodyType.PlayerBullet && objectB.PhysicsBodyType == PhysicsBodyType.PowerUpBall;
            var isPowerUpToBullet = objectA.PhysicsBodyType == PhysicsBodyType.PowerUpBall && objectB.PhysicsBodyType == PhysicsBodyType.PlayerBullet;
            var isCollision = isBulletToPowerUp || isPowerUpToBullet;

            if (!isCollision)
            {
                return;
            }
            
            if (!TryGetPowerUpBallToBulletCollisionData(objectA, objectB, contact, isBulletToPowerUp, out var bulletModel, out int powerUpBallIndex, out var bulletBody, out var powerUpBody))
            {
                return;
            }

            ref var powerUpBall = ref _matchDataService.SimulationState.PowerUpBalls.GetByIndex(powerUpBallIndex);
            var powerUpBallId = powerUpBall.Id;
            DestroyBullet(bulletModel, bulletBody);
            DestroyPowerUpBall(powerUpBallId, powerUpBody);
            _netEventsDataService.AddPowerUpObtainedNetEvent(_processedTick, powerUpBallId, bulletModel.BelongToPlayerId);
        }

        private void DestroyPowerUpBall(ushort powerUpBallId, Body powerUpBallBody)
        {
            _matchDataService.SimulationState.RemovePowerUpBallById(powerUpBallId);
            _physicsSimulator.RemoveBody(powerUpBallBody);
        }

        private bool TryGetPowerUpBallToBulletCollisionData(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact, bool isBulletToPowerUpCollision, out PlayerBulletS2C bulletModel, out int powerUpBallIndex, out Body bulletBody, out Body powerUpBallBody)
        {
            ushort bulletId;
            ushort powerUpBallId;
            powerUpBallIndex = default;
            powerUpBallBody = default;
            bulletModel = default;
            
            if (isBulletToPowerUpCollision)
            {
                bulletId = objectA.Id;
                powerUpBallId = objectB.Id;
                bulletBody = contact.FixtureA.Body;
                powerUpBallBody = contact.FixtureB.Body;
            }
            else
            {
                bulletId = objectB.Id;
                powerUpBallId = objectA.Id;
                bulletBody = contact.FixtureB.Body;
                powerUpBallBody = contact.FixtureA.Body;
            }

            if (!_matchDataService.SimulationState.TryGetBulletById(bulletId, out bulletModel))
            {
                LogService.LogTopic($"Bullet {bulletId} was already destroyed in this frame!", LogTopicType.ServerPhysics);
                return false;
            }
            
            if (!_matchDataService.SimulationState.TryGetPowerUpBallIndexById(powerUpBallId, out powerUpBallIndex))
            {
                LogService.LogTopic($"PowerUpBall {powerUpBallId} was already destroyed in this frame!", LogTopicType.ServerPhysics);
                return false;
            }
            
            return true;
        }
    }
}