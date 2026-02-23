using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class InitStageCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private SimulationGamePlayConfig _gamePlayConfig;
        private IStageDataService _stageDataService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private ITeleportGateService _teleportGateService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private ITickService _tickService;
        private NetworkConfig _networkConfig;
        private IMatchEnvironmentConfigDataService _matchEnvironmentConfigDataService;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _teleportGateService = _diContainer.Resolve<ITeleportGateService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _tickService = _diContainer.Resolve<ITickService>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _matchEnvironmentConfigDataService = _diContainer.Resolve<IMatchEnvironmentConfigDataService>();
        }

        public void Execute()
        {
            _physicsSimulator.ClearAllData();
            _playersInLavaTrackerService.ClearAllData();
            _teleportGateService.ClearData();
            ClearStageObjectsInSimulationState();
            _matchEnvironmentConfigDataService.InitEnvironmentLayout(_gamePlayConfig.ChosenEnvironmentIndex);

            CreateWalls();
            CreateLavaWalls();
            CreateTalentCards();
            CreateEnvironmentSprings();
            CreateTeleportGates();
            CreateRotatingWheels();
            ResetPlayers();

            _stageDataService.IsStageEnded = false;
            _stageDataService.StageRestartTimer = -1;
            _stageDataService.ClearData();
        }

        private void ClearStageObjectsInSimulationState()
        {
            _matchDataService.SimulationState.Bullets.Clear();
            _matchDataService.SimulationState.PowerUpBalls.Clear();
            _matchDataService.SimulationState.TalentCards.Clear();
        }

        private void ResetPlayers()
        {
            var halfSize = _matchEnvironmentConfigDataService.EnvironmentHalfSize;
            var players = _matchDataService.SimulationState.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players.GetByIndex(i);

                var health = _gamePlayConfig.PlayerSpaceship.StartHealth;
                var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
                var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;

                player.Spaceship.Health.CurrentHealth = health;
                player.Spaceship.Health.MaxHealth = health;
                player.Spaceship.Shoot.CooldownSecondsLeft = shootCooldown;
                player.Spaceship.Shoot.MaxCooldown = shootCooldown;

                var position = GetRandomFreePosition(radius, halfSize);
                var direction = RNG.NextFloat(0, 360).AngleToVector();
                var velocity = direction * _gamePlayConfig.PlayerSpaceship.TargetMovementSpeed;

                player.Spaceship.Transform.Position = position;
                player.Spaceship.Transform.Direction = direction;
                player.Spaceship.Transform.Velocity = velocity;
                player.Spaceship.Transform.Radius = radius;
                player.Spaceship.IsEngineOn = true;
                player.Spaceship.IsAlive = true;
                
                _physicsSimulator.AddPlayer(player.Id, player.TeamId, position, velocity, radius);
            }
        }

        private Vector2 GetRandomFreePosition(float radius, Vector2 halfSize) // todo: in each environemnt we should have spawn points, and just choose one of them
        {
             for (int i = 0; i < 100; i++)
             {
                 var x = RNG.NextFloat(-halfSize.X + radius, halfSize.X - radius);
                 var y = RNG.NextFloat(-halfSize.Y + radius, halfSize.Y - radius);
                 var pos = new Vector2(x, y);

                 if (!_physicsSimulator.IsSquareHitAnyBodyTypes(pos, radius, PhysicsBodyType.Wall, PhysicsBodyType.Lava, PhysicsBodyType.StartMatchWall))
                 {
                     return pos;
                 }
             }
             LogService.LogError("No free position found!");
             return Vector2.Zero;
        }

        private void CreateWalls()
        {
            var wallConfigs = _matchEnvironmentConfigDataService.WallConfigs;

            foreach (var wallConfig in wallConfigs)
            {
                AddWallToEnvironment(wallConfig);
            }
        }

        private void AddWallToEnvironment(WallConfig wallConfig)
        {
            var wallId = wallConfig.Id;
            var wallPoints = wallConfig.Points;
            _matchDataService.EnvironmentData.AddWall(wallId, Vector2.Zero, wallPoints);
            _physicsSimulator.AddWall(wallId, wallPoints);
        }

        private void CreateLavaWalls()
        {
            var lavaWallConfigs = _matchEnvironmentConfigDataService.LavaWallConfigs;
            if (lavaWallConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var lavaWallConfig in lavaWallConfigs)
            {
                AddLavaWallToEnvironment(lavaWallConfig);
            }
        }

        private void AddLavaWallToEnvironment(WallConfig lavaWallConfig)
        {
            var lavaWallId = lavaWallConfig.Id;
            var lavaWallPoints = lavaWallConfig.Points;
            _matchDataService.EnvironmentData.AddLavaWall(lavaWallId, Vector2.Zero, lavaWallPoints);
            _physicsSimulator.AddLavaWall(lavaWallId, lavaWallPoints);
        }

        private void CreateTalentCards()
        {
            var talentCards = _matchEnvironmentConfigDataService.TalentCards;
            if (talentCards.IsNullOrEmpty())
            {
                return;
            }

            foreach (var talentCard in talentCards)
            {
                var talentCardPosition = talentCard.Position;
                var talentCardId = talentCard.Id;
                _matchDataService.AddTalentCard(talentCardId, talentCardPosition, talentCard.TalentType, _gamePlayConfig.Talents.TalentCardHealth);
                _physicsSimulator.AddTalentCard(talentCardId, talentCardPosition, _gamePlayConfig.Talents.TalentCardWidth, _gamePlayConfig.Talents.TalentCardHeight);
            }
        }

        private void CreateEnvironmentSprings()
        {
            var environmentSprings = _matchEnvironmentConfigDataService.EnvironmentSprings;
            if (environmentSprings.IsNullOrEmpty())
            {
                return;
            }

            foreach (var environmentSpring in environmentSprings)
            {
                AddSpringToEnvironment(environmentSpring.Id, Vector2.Zero, environmentSpring.Position, 0, environmentSpring.RotationAngle);
            }
        }

        private void AddSpringToEnvironment(ushort springId, Vector2 springLocalPosition, Vector2 springWorldPosition, float springLocalRotationAngle, float springWorldRotationAngle)
        {
            var springSize = _gamePlayConfig.EnvironmentSprings.Size.ToNumericsVector2();
            _matchDataService.EnvironmentData.AddSpring(springId, springLocalPosition, springWorldPosition, springLocalRotationAngle, springWorldRotationAngle);
            _physicsSimulator.AddEnvironmentSpring(springId, springWorldPosition, springWorldRotationAngle, springSize);
        }

        private void CreateTeleportGates()
        {
            var teleportGatePairConfigs = _matchEnvironmentConfigDataService.TeleportGates;
            if (teleportGatePairConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var teleportGatePairConfig in teleportGatePairConfigs)
            {
                var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size.ToNumericsVector2();
                _matchDataService.EnvironmentData.AddTeleportGatePair(teleportGatePairConfig.Id, teleportGatePairConfig.GateAId, teleportGatePairConfig.GateBId, teleportGatePairConfig.GateA.Position, teleportGatePairConfig.GateA.NormalRotation, teleportGatePairConfig.GateB.Position, teleportGatePairConfig.GateB.NormalRotation);
                _physicsSimulator.AddTeleportGate(teleportGatePairConfig.GateAId, teleportGatePairConfig.GateA.Position, teleportGatePairConfig.GateA.NormalRotation, gateSize);
                _physicsSimulator.AddTeleportGate(teleportGatePairConfig.GateBId, teleportGatePairConfig.GateB.Position, teleportGatePairConfig.GateB.NormalRotation, gateSize);
            }
        }

        private void CreateRotatingWheels()
        {
            var rotatingWheelsConfigs = _matchEnvironmentConfigDataService.RotatingWheels;
            if (rotatingWheelsConfigs.IsNullOrEmpty())
            {
                return;
            }

            var currentTick = _tickService.CurrentTick;
            var deltaTime = _networkConfig.DeltaTime;
            
            foreach (var wheelConfig in rotatingWheelsConfigs)
            {
                var wheelCenter = wheelConfig.CenterPosition;
                var rotationSpeed = wheelConfig.RotationSpeed;
                var rotatingWheel = _matchDataService.EnvironmentData.AddRotatingWheel(wheelConfig.Id, wheelCenter, rotationSpeed);

                if (!wheelConfig.Walls.IsNullOrEmpty())
                {
                    foreach (var wallConfig in wheelConfig.Walls)
                    {
                        AddWallToEnvironment(wallConfig);
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            currentTick, rotationSpeed, deltaTime, wheelCenter, Vector2.Zero, 0,
                            out var newPosition, out var newRotation
                        );
                        
                        var wallId = wallConfig.Id;
                        rotatingWheel.AddWall(wallId);
                        _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.Wall, wallId, newPosition, newRotation);
                    }
                }

                if (!wheelConfig.LavaWalls.IsNullOrEmpty())
                {
                    foreach (var lavaWallConfig in wheelConfig.LavaWalls)
                    {
                        AddLavaWallToEnvironment(lavaWallConfig);
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            currentTick, rotationSpeed, deltaTime, wheelCenter, Vector2.Zero, 0,
                            out var initialPos, out var initialRot
                        );

                        var lavaWallId = lavaWallConfig.Id;
                        rotatingWheel.AddLavaWall(lavaWallId);
                        _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.Lava, lavaWallId, initialPos, initialRot);
                    }
                }

                if (!wheelConfig.Springs.IsNullOrEmpty())
                {
                    foreach (var springConfig in wheelConfig.Springs)
                    {
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            currentTick, rotationSpeed, deltaTime, wheelCenter, springConfig.Position, springConfig.RotationAngle,
                            out var worldPosition, out var worldRotation);

                        var springId = springConfig.Id;
                        AddSpringToEnvironment(springId, springConfig.Position, worldPosition, springConfig.RotationAngle, worldRotation);
                        rotatingWheel.AddSpring(springId);
                    }
                }
            }
        }
    }
}
