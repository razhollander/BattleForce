using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class ProcessCachedCollisionsCommand : BaseCommand, ICommandVoid
    {
        private IPhysicsSimulator _physicsSimulator;
        private IMatchDataService _matchDataService;
        private ICommandFactory _commandFactory;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private INetEventsDataService _netEventsDataService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private IPlayersTalentsManager _playersTalentsManager;
        private ITeleportGateService _teleportGateService;
        private IPlayersOutsideStageTrackerService _playersOutsideStageTrackerService;
        private IPlayersTouchingWallDataService _playersTouchingWallDataService;

        private int _processedTick;
        private PlayerHitCommand _playerHitCommand;
        private SpinPlayerCommand _spinPlayerCommand;
        private ObtainPowerUpBallCommand _obtainPowerUpBallCommand;

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
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _playerHitCommand = _commandFactory.CreateCommandVoid<PlayerHitCommand>();
            _spinPlayerCommand = _commandFactory.CreateCommandVoid<SpinPlayerCommand>();
            _obtainPowerUpBallCommand = _commandFactory.CreateCommandVoid<ObtainPowerUpBallCommand>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _playersOutsideStageTrackerService = _diContainer.Resolve<IPlayersOutsideStageTrackerService>();
            _playersTouchingWallDataService = _diContainer.Resolve<IPlayersTouchingWallDataService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _teleportGateService = _diContainer.Resolve<ITeleportGateService>();
        }

        public void Execute()
        {
            ProcessCollisions();
        }

        private void ProcessCollisions()
        {
            var cachedCollisions = _physicsSimulator.GetCachedCollisions();

            for (int i = 0; i < cachedCollisions.Count; i++) // This must stay for and not forearch! since if we destroy and object an event 'ContactEnd' will be added
            {
                var collisionEvent = cachedCollisions[i];

                var objectA = collisionEvent.BodyDataA;
                var objectB = collisionEvent.BodyDataB;
                var isPlayer = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.Wall;
                var isPlayer2 = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

                if (isPlayer||isPlayer2)
                {
                    var playerId = isPlayer ? objectA.Id : objectB.Id;
                    var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);

                    if (playerState.Name == "Alon")
                    {
                        LogService.LogError($"Collision: {collisionEvent.Type}");
                    }
                }
                HandlePlayerLavaCollision(objectA, objectB, collisionEvent.Type);
                HandlePlayerStageBoundaryCollision(objectA, objectB, collisionEvent.Type);
                HandlePlayerWallStickTracking(objectA, objectB, collisionEvent.Type, collisionEvent.Contact);

                if (collisionEvent.Type != PhysicsEventEventType.Begin)
                {
                    continue;
                }

                HandlePlayerWallCollision(objectA, objectB, collisionEvent.Contact);
                HandlePowerUpBallWallCollision(objectA, objectB, collisionEvent.Contact);
                HandleBulletWallCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerHeartBulletCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletTalentCardCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerBulletPowerUpCollision(objectA, objectB, collisionEvent.Contact);
                HandlePlayerEnvironmentSpringCollision(objectA, objectB);
                HandlePlayerEnvironmentSpikeCollision(objectA, objectB);
                HandlePlayerTeleportGateCollision(objectA, objectB);
                HandleSwapFieldPlayerCollision(objectA, objectB);
                HandleKOProjectilePlayerCollision(objectA, objectB);
                HandleKOProjectileWallCollision(objectA, objectB);
                HandleGrapplingHookWallCollision(objectA, objectB);
                HandleChickenEggPlayerCollision(objectA, objectB);
                HandleChickenEggKOProjectileCollision(objectA, objectB);
            }

            _physicsSimulator.ClearCachedCollisions();
        }

        private void HandleChickenEggKOProjectileCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            bool isEggToKOProjectile = objectA.PhysicsBodyType == PhysicsBodyType.ChickenEgg && objectB.PhysicsBodyType == PhysicsBodyType.KOProjectile;
            bool isKOProjectileToEgg = objectB.PhysicsBodyType == PhysicsBodyType.ChickenEgg && objectA.PhysicsBodyType == PhysicsBodyType.KOProjectile;

            if (!isEggToKOProjectile && !isKOProjectileToEgg)
            {
                return;
            }

            ushort eggId = isEggToKOProjectile ? objectA.Id : objectB.Id;
            ushort koProjectileId = isEggToKOProjectile ? objectB.Id : objectA.Id;

            if (!_matchDataService.SimulationState.TryGetChickenEggById(eggId, out var egg))
            {
                return;
            }

            if (!_matchDataService.SimulationState.TryGetKOProjectileById(koProjectileId, out var koProjectile))
            {
                return;
            }
            
            var eggTeam = _matchDataService.SimulationState.GetPlayerById(egg.PlayerCasterId).TeamId;
            var koProjectileTeam = _matchDataService.SimulationState.GetPlayerById(koProjectile.PlayerCasterId).TeamId;
            var areFromTheSameTeam = koProjectileTeam == eggTeam;

            if (areFromTheSameTeam)
            {
                return;
            }
            
            _netEventsDataService.AddChickenEggHitNetEventS2C(_processedTick, eggId);
            _physicsSimulator.RemoveChickenEgg(egg.Id);
            _matchDataService.SimulationState.RemoveChickenEggById(egg.Id);
        }

        private void HandleGrapplingHookWallCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            var isWallToProjectile = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.GrapplingHookProjectile;
            var isProjectileToWall = objectA.PhysicsBodyType == PhysicsBodyType.GrapplingHookProjectile && objectB.PhysicsBodyType == PhysicsBodyType.Wall;

            if (!isWallToProjectile && !isProjectileToWall)
            {
                return;
            }

            var projectileId = isProjectileToWall ? objectA.Id : objectB.Id;
            var wallId = isProjectileToWall ? objectB.Id : objectA.Id;

            if (!_matchDataService.SimulationState.TryGetGrapplingHookProjectileById(projectileId, out var projectile))
            {
                return;
            }

            _playersTalentsManager.HitGrapplingHookWithWall(projectile.PlayerCasterId, projectileId, wallId, _processedTick);
        }

        private void HandleChickenEggPlayerCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            bool isEggToPlayer = objectA.PhysicsBodyType == PhysicsBodyType.ChickenEgg && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;
            bool isPlayerToEgg = objectB.PhysicsBodyType == PhysicsBodyType.ChickenEgg && objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isEggToPlayer && !isPlayerToEgg)
            {
                return;
            }

            ushort eggId = isEggToPlayer ? objectA.Id : objectB.Id;
            ushort playerId = isEggToPlayer ? objectB.Id : objectA.Id;

            if (!_matchDataService.SimulationState.TryGetChickenEggById(eggId, out var egg))
            {
                return;
            }

            var player = _matchDataService.SimulationState.GetPlayerById(playerId);
            var areFromTheSameTeam = player.TeamId == _matchDataService.SimulationState.GetPlayerById(egg.PlayerCasterId).TeamId;

            if (areFromTheSameTeam)
            {
                return;
            }

            var config = _gamePlayConfigService.GamePlayConfig.Talents.ChickenTalentConfig;
            _spinPlayerCommand.SetPlayer(player.Id).SetSpinAmount(config.SpinAmount).SetTick(_processedTick).Execute();
            
            _netEventsDataService.AddChickenEggHitNetEventS2C(_processedTick, eggId);
            _physicsSimulator.RemoveChickenEgg(egg.Id);
            _matchDataService.SimulationState.RemoveChickenEggById(egg.Id);
        }

        private void HandleKOProjectileWallCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            var isWallToProjectile = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.KOProjectile;
            var isProjectileToWall = objectA.PhysicsBodyType == PhysicsBodyType.KOProjectile && objectB.PhysicsBodyType == PhysicsBodyType.Wall;

            if (!isWallToProjectile && !isProjectileToWall)
            {
                return;
            }

            var projectileId = isProjectileToWall ? objectA.Id : objectB.Id;
            if (!_matchDataService.SimulationState.TryGetKOProjectileById(projectileId, out var koProjectile))
            {
                LogService.LogTopic("Ko Projectile was already destroyed in this frame!", LogTopicType.ServerPhysics);
                return;
            }
            
            var casterId = koProjectile.PlayerCasterId;
            _playersTalentsManager.HitKOTalentWithWall(casterId);
        }

        private void HandleKOProjectilePlayerCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            var isPlayerToProjectile = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.KOProjectile;
            var isProjectileToPlayer = objectA.PhysicsBodyType == PhysicsBodyType.KOProjectile && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerToProjectile && !isProjectileToPlayer)
            {
                return;
            }

            ushort playerId;
            ushort projectileId;

            if (isPlayerToProjectile)
            {
                playerId = objectA.Id;
                projectileId = objectB.Id;
            }
            else
            {
                projectileId = objectA.Id;
                playerId = objectB.Id;
            }

            if (!_matchDataService.SimulationState.TryGetKOProjectileById(projectileId, out var koProjectile))
            {
                LogService.LogTopic("Ko Projectile was already destroyed in this frame!", LogTopicType.ServerPhysics);
                return;
            }
            
            var enemyPlayer = _matchDataService.SimulationState.GetPlayerById(playerId);
            _playersTalentsManager.HitKOTalentWithEnemy(koProjectile.PlayerCasterId, enemyPlayer.Id, _processedTick);
        }
        
        private void HandleSwapFieldPlayerCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            var isPlayerToField = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.SwapField;
            var isFieldToPlayer = objectA.PhysicsBodyType == PhysicsBodyType.SwapField && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerToField && !isFieldToPlayer)
            {
                return;
            }

            ushort playerId;
            ushort fieldId;

            if (isPlayerToField)
            {
                playerId = objectA.Id;
                fieldId = objectB.Id;
            }
            else
            {
                playerId = objectB.Id;
                fieldId = objectA.Id;
            }
            
            if (!_matchDataService.SimulationState.TryGetSwapFieldById(fieldId, out var swapField))
            {
                LogService.Log("Swap field already collided with another player this tick therefore was destroyed!");
                return;   
            }

            var playerStateHit = _matchDataService.SimulationState.GetPlayerById(playerId);
            _playersTalentsManager.CompleteSwapTalentWithEnemy(swapField.PlayerCasterId, playerStateHit.Id, _processedTick);
        }

        private void HandlePlayerTeleportGateCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            var isPlayerToGate = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.EnvironmentTeleportGate;
            var isGateToPlayer = objectA.PhysicsBodyType == PhysicsBodyType.EnvironmentTeleportGate && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerToGate && !isGateToPlayer)
            {
                return;
            }

            ushort playerId;
            ushort gateBodyId;

            if (isPlayerToGate)
            {
                playerId = objectA.Id;
                gateBodyId = objectB.Id;
            }
            else
            {
                playerId = objectB.Id;
                gateBodyId = objectA.Id;
            }

            if (_teleportGateService.IsTeleportOnCooldown(playerId, _processedTick))
            {
                return;
            }

            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var teleportPairData = _matchDataService.EnvironmentData.GetTeleportGatePairOfGate(gateBodyId);
            var isGateB = teleportPairData.GateB.Id == gateBodyId;
            var enterGatePosition = isGateB ? teleportPairData.GateB.Transform.WorldPosition : teleportPairData.GateA.Transform.WorldPosition;
            var enterGateRotation = isGateB ? teleportPairData.GateB.Transform.WorldRotationDegrees : teleportPairData.GateA.Transform.WorldRotationDegrees;
            var exitGatePosition = isGateB ? teleportPairData.GateA.Transform.WorldPosition : teleportPairData.GateB.Transform.WorldPosition;
            var exitGateRotation = isGateB ? teleportPairData.GateA.Transform.WorldRotationDegrees : teleportPairData.GateB.Transform.WorldRotationDegrees;
            var enterPoint = playerState.Spaceship.Transform.Position;

            var enterGateNormal = enterGateRotation.ToRadians().AngleToVector();
            var exitGateNormal = exitGateRotation.ToRadians().AngleToVector();
            var exitPoint = MathUtils.TeleportsLogic.GetRelativeExitPoint(enterPoint, enterGatePosition, enterGateNormal, exitGatePosition, exitGateNormal);
            playerState.Spaceship.Transform.Position = exitPoint;
            var newDirection = MathUtils.TeleportsLogic.ConvertVectorTelativeToExitTeleport(playerState.Spaceship.Transform.Direction, enterGateNormal, exitGateNormal);
            playerState.Spaceship.Transform.Direction = newDirection;
            var newVelocity = MathUtils.TeleportsLogic.ConvertVectorTelativeToExitTeleport(playerState.Spaceship.Transform.Velocity, enterGateNormal, exitGateNormal);
            playerState.Spaceship.Transform.Velocity = newVelocity;
            
            _teleportGateService.RegisterTeleport(playerId, _processedTick);
            _netEventsDataService.AddPlayerToEnvironmentTeleportGateCollisionNetEvent(_processedTick, teleportPairData.Id, enterPoint, exitPoint, playerId);
        }

        private void HandlePlayerEnvironmentSpringCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            var isPlayerToSpring = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.EnvironmentSpring;
            var isSpringToPlayer = objectA.PhysicsBodyType == PhysicsBodyType.EnvironmentSpring && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerToSpring && !isSpringToPlayer)
            {
                return;
            }

            ushort playerId;
            ushort springId;

            if (isPlayerToSpring)
            {
                playerId = objectA.Id;
                springId = objectB.Id;
            }
            else
            {
                playerId = objectB.Id;
                springId = objectA.Id;
            }

            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var springAngle = _matchDataService.EnvironmentData.GetSpring(springId).WorldDirectionDegrees.ToRadians();
            var pushDirection = springAngle.FromAngleRadians();
            var environmentSpringsConfig = _gamePlayConfigService.GamePlayConfig.EnvironmentSprings;
            var forceMagnitude = environmentSpringsConfig.Force * _matchDataService.SimulationState.MapSizeMultiplier;
            var force = pushDirection * forceMagnitude;
            var randomSpin = RNG.NextFloat(environmentSpringsConfig.MinSpin, environmentSpringsConfig.MaxSpin);

            playerState.Spaceship.Transform.Velocity += force;
            playerState.Spaceship.Transform.Direction = force.NormalizeSafe();
            playerState.Spaceship.IsEngineOn = false;

            _spinPlayerCommand
                .SetPlayer(playerId)
                .SetSpinAmount(randomSpin)
                .SetTick(_processedTick)
                .Execute();

            _netEventsDataService.AddEnvironmentSpringPlayerCollisionNetEvent(_processedTick, springId, playerId, pushDirection);
        }

        private void HandlePlayerEnvironmentSpikeCollision(PhysicsBodyData objectA, PhysicsBodyData objectB)
        {
            var isPlayerToSpike = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.EnvironmentSpike;
            var isSpikeToPlayer = objectA.PhysicsBodyType == PhysicsBodyType.EnvironmentSpike && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerToSpike && !isSpikeToPlayer)
            {
                return;
            }

            ushort playerId;
            ushort spikeId;

            if (isPlayerToSpike)
            {
                playerId = objectA.Id;
                spikeId = objectB.Id;
            }
            else
            {
                playerId = objectB.Id;
                spikeId = objectA.Id;
            }

            if (!_matchDataService.SimulationState.GetPlayerById(playerId).Spaceship.IsAlive)
            {
                return;
            }

            var damage = _gamePlayConfigService.GamePlayConfig.EnvironmentSpikes.Damage;
            _playerHitCommand
                .SetPlayerIdGotHit(playerId)
                .SetHitDamage(damage)
                .SetProcessedTick(_processedTick)
                .Execute();

            _netEventsDataService.AddEnvironmentSpikePlayerCollisionNetEvent(_processedTick, spikeId, playerId);
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

        private void HandlePlayerStageBoundaryCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, PhysicsEventEventType eventType)
        {
            var isPlayerStageBoundary = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.StageBoundary;
            var isStageBoundaryPlayer = objectA.PhysicsBodyType == PhysicsBodyType.StageBoundary && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerStageBoundary && !isStageBoundaryPlayer)
            {
                return;
            }

            var playerId = isPlayerStageBoundary ? objectA.Id : objectB.Id;

            if (eventType == PhysicsEventEventType.Begin)
            {
                _playersOutsideStageTrackerService.OnPlayerEnterStageBoundary(playerId);
            }
            else if (eventType == PhysicsEventEventType.End)
            {
                _playersOutsideStageTrackerService.OnPlayerExitStageBoundary(playerId);
            }
        }

        private void HandlePlayerWallStickTracking(PhysicsBodyData objectA, PhysicsBodyData objectB, PhysicsEventEventType eventType, Contact contact)
        {
            var isPlayerToWall = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.Wall;
            var isWallToPlayer = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;

            if (!isPlayerToWall && !isWallToPlayer)
            {
                return;
            }

            var playerId = isPlayerToWall ? objectA.Id : objectB.Id;
            var wallId = isPlayerToWall ? objectB.Id : objectA.Id;

            if (eventType == PhysicsEventEventType.Begin)
            {
                contact.GetWorldManifold(out var worldManifold);
                _playersTouchingWallDataService.OnPlayerBeginTouchWall(playerId, wallId, worldManifold.normal, _processedTick);
            }
            else if (eventType == PhysicsEventEventType.End)
            {
                _playersTouchingWallDataService.OnPlayerEndTouchWall(playerId, wallId);
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
            
            Body bulletBody;
            PlayerBulletS2C bulletModel;
            if (isPlayerToBulletCollision)
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
        
        private void HandlePlayerHeartBulletCollision(PhysicsBodyData objectA, PhysicsBodyData objectB, Contact contact)
        {
            var isBulletToHeartCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerBullet && objectB.PhysicsBodyType == PhysicsBodyType.PlayerHeart;
            var isHeartToBulletCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerHeart && objectB.PhysicsBodyType == PhysicsBodyType.PlayerBullet;
            var isCollision = isHeartToBulletCollision || isBulletToHeartCollision;
            if (!isCollision)
            {
                return;
            }
            
            ushort playerId;
            Body bulletBody;
            PlayerBulletS2C bulletModel;
            if (isHeartToBulletCollision)
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

            var wasBulletCreatedOnTopOfPlayerHeart = bulletModel.CreatedOnTick == _processedTick;
            if (wasBulletCreatedOnTopOfPlayerHeart)
            {
                return;
            }
            
            _playerHitCommand
                .SetPlayerIdGotHit(playerId)
                .SetWasHitByAnotherPlayer(true, bulletModel.BelongToPlayerId)
                .SetHitDamage(_gamePlayConfigService.GamePlayConfig.PlayerBullet.HitDamage)
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
            talentCard.Health -= _gamePlayConfigService.GamePlayConfig.PlayerBullet.HitDamage;
            var isTalentCardAlive = talentCard.Health > 0;

            if (isTalentCardAlive)
            {
                _netEventsDataService.AddTalentCardHitNetEvent(_processedTick, talentCard.Id, talentCard.Health);
            }
            else
            {
                var hitByPlayerId = bulletModel.BelongToPlayerId;

                if (_playersTalentsManager.TryAddTalentToPlayer(talentCard.TalentType, hitByPlayerId, _processedTick, out _, out bool didReplaceExistingTalent))
                {
                    var playerTalents = _matchDataService.SimulationState.GetPlayerById(hitByPlayerId).Spaceship.TalentsState.Talents;
                    _netEventsDataService.AddTalentCardObtainedNetEvent(_processedTick, talentCard.Id, hitByPlayerId, playerTalents, didReplaceExistingTalent);
                    DestroyTalentCard(talentCard, cardBody);
                }
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
            var currentDirection = playerModel.Spaceship.Transform.Direction;
            var reflectedDirection = currentDirection.ReflectFromWall(collisionNormal);
            playerModel.Spaceship.Transform.Direction = reflectedDirection.Length() > 0
                ? reflectedDirection.NormalizeSafe()
                : currentDirection;
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
            _obtainPowerUpBallCommand
                .SetProcessedTick(_processedTick)
                .SetPowerUpBallId(powerUpBallId)
                .SetObtainedByPlayerId(bulletModel.BelongToPlayerId)
                .Execute();
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