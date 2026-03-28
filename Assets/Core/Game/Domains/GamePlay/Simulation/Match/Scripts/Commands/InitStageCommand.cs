using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;

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
        private NetworkConfig _networkConfig;
        private IMatchEnvironmentConfigDataService _matchEnvironmentConfigDataService;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        private IPlayersTalentsManager _playersTalentsManager;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _teleportGateService = _diContainer.Resolve<ITeleportGateService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _matchEnvironmentConfigDataService = _diContainer.Resolve<IMatchEnvironmentConfigDataService>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
        }

        public void Execute()
        {
            LogService.LogError("init stage on server side");
            ClearStageData();
            
            CreateEnvironmentLayout();
            SetupPlayers();
        }

        private void CreateEnvironmentLayout()
        {
            var environmentLayoutIndex = _gamePlayConfig.ChosenEnvironmentIndex;
            _matchDataService.SimulationState.EnvironmentLayoutIndex = environmentLayoutIndex;
            _matchEnvironmentConfigDataService.InitEnvironmentLayout(environmentLayoutIndex);
            
            CreateWalls();
            CreateLavaWalls();
            CreateTalentCards();
            CreateEnvironmentSprings();
            CreateTeleportGates();
            CreateRotatingWheels();
            CreateFieldBarriers();
        }

        private void ClearStageData()
        {
            _physicsSimulator.ClearAllData();
            _playersInLavaTrackerService.ClearAllData();
            _teleportGateService.ClearData();
            ClearStageObjectsInSimulationState();
            _matchDataService.SimulationState.IsInPreparationPhase = true;
            _matchDataService.SimulationState.StartPhaseInitialTick = 0;
            _playersTalentsManager.ResetAllTalentsData();
            _preparationPhaseTimerService.RestartTimer();
            _stageDataService.ClearData();
        }

        private void ClearStageObjectsInSimulationState()
        {
            _matchDataService.SimulationState.Bullets.Clear();
            _matchDataService.SimulationState.PowerUpBalls.Clear();
            _matchDataService.SimulationState.TalentCards.Clear();
            _matchDataService.SimulationState.SwapFields.Clear();
            _matchDataService.EnvironmentData.ClearData();
        }

        private void SetupPlayers()
        {
            var halfSize = _matchEnvironmentConfigDataService.EnvironmentHalfSize;
            var players = _matchDataService.SimulationState.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players.GetByIndex(i);

                var health = _gamePlayConfig.PlayerSpaceship.StartHealth;
                var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
                var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;

                var teamId = player.TeamId;
                Vector2 position;

                var barrier = GetBarrierForTeam(teamId);
                if (barrier != null)
                {
                    position = barrier.Position;
                }
                else
                {
                    position = GetRandomFreePosition(radius, halfSize);
                }

                var direction = RNG.NextFloat(0, 360).AngleToVector();
                var velocity = direction * _gamePlayConfig.PlayerSpaceship.TargetMovementSpeed;

                player.Spaceship.Health.CurrentHealth = health;
                player.Spaceship.Health.MaxHealth = health;
                player.Spaceship.Shoot.CooldownSecondsLeft = shootCooldown;
                player.Spaceship.Shoot.MaxCooldown = shootCooldown;
                player.Spaceship.Transform.Position = position;
                player.Spaceship.Transform.Direction = direction;
                player.Spaceship.Transform.Velocity = velocity;
                player.Spaceship.Transform.Radius = radius;
                player.Spaceship.IsEngineOn = true;
                player.Spaceship.IsAlive = true;

                var talentsCount = player.Spaceship.TalentsState.Talents.Count;
                for (var k = 0; k < talentsCount; k++)
                {
                    ref var talentState = ref player.Spaceship.TalentsState.Talents.Get(k);
                    talentState.ClearCooldown();
                }
                
                _physicsSimulator.AddPlayer(player.Id, player.TeamId, position, velocity, radius);
            }
        }

        private Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel.MatchEnvironmentFieldBarrierModel GetBarrierForTeam(ushort teamId)
        {
            foreach (var barrier in _matchDataService.EnvironmentData.FieldBarriers.AsSpan())
            {
                if (barrier.TeamId == teamId)
                {
                    return barrier;
                }
            }
            return null;
        }

        private void CreateFieldBarriers()
        {
            var barrierConfigs = _matchEnvironmentConfigDataService.FieldBarrierConfigs;
            if (barrierConfigs.IsNullOrEmpty())
            {
                return;
            }

            var teamIds = new System.Collections.Generic.List<ushort>(_matchDataService.TeamIds);
            teamIds.Sort();

            int barrierIndex = 0;
            foreach (var teamId in teamIds)
            {
                if (barrierIndex >= barrierConfigs.Length)
                {
                    break;
                }

                var config = barrierConfigs[barrierIndex];
                _matchDataService.EnvironmentData.AddFieldBarrier((ushort)barrierIndex, teamId, config.Position, config.Size, config.Shape);
                barrierIndex++;
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
                AddWallToEnvironment(wallConfig.Id, wallConfig.Points, wallConfig.Position, wallConfig.Position, 0);
            }
        }

        private void AddWallToEnvironment(ushort wallId, Vector2[] wallPoints, Vector2 lavaWallLocalPosition, Vector2 lavaWallWorldPosition, float lavaWallWorldRotationAngle)
        {
            _matchDataService.EnvironmentData.AddWall(wallId, wallPoints, lavaWallLocalPosition, lavaWallWorldPosition, lavaWallWorldRotationAngle);
            _physicsSimulator.AddWall(wallId, wallPoints, lavaWallWorldPosition);
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
                AddLavaWallToEnvironment(lavaWallConfig.Id, lavaWallConfig.Points, lavaWallConfig.Position, lavaWallConfig.Position, 0);
            }
        }

        private void AddLavaWallToEnvironment(ushort lavaWallId, Vector2[] lavaWallPoints, Vector2 lavaWallLocalPosition, Vector2 lavaWallWorldPosition, float lavaWallWorldRotationAngle)
        {
            _matchDataService.EnvironmentData.AddLavaWall(lavaWallId, lavaWallPoints, lavaWallLocalPosition, lavaWallWorldPosition, lavaWallWorldRotationAngle);
            _physicsSimulator.AddLavaWall(lavaWallId, lavaWallPoints, lavaWallWorldPosition);
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
                AddTeleportGatePairToEnvironment(teleportGatePairConfig.Id, teleportGatePairConfig.GateAId, teleportGatePairConfig.GateBId, Vector2.Zero, 0, Vector2.Zero, 0,
                    teleportGatePairConfig.GateA.Position, teleportGatePairConfig.GateA.NormalRotation, teleportGatePairConfig.GateB.Position,
                    teleportGatePairConfig.GateB.NormalRotation);
            }
        }
        
        private void AddTeleportGatePairToEnvironment(ushort teleportPairId, ushort gateAId, ushort gateBId, Vector2 gateAPosition, float gateANormalRotation, Vector2 gateBPosition,
            float gateBNormalRotation, Vector2 gateAWorldPosition, float gateAWorldRotation, Vector2 gateBWorldPosition, float gateBWorldRotation)
        {
            var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size.ToNumericsVector2();
            _matchDataService.EnvironmentData.AddTeleportGatePair(teleportPairId, gateAId, gateBId, gateAPosition, gateANormalRotation, gateBPosition, gateBNormalRotation,
                gateAWorldPosition, gateAWorldRotation, gateBWorldPosition, gateBWorldRotation);
            _physicsSimulator.AddTeleportGate(gateAId, gateAWorldPosition, gateAWorldRotation, gateSize);
            _physicsSimulator.AddTeleportGate(gateBId, gateBWorldPosition, gateBWorldRotation, gateSize);
        }

        private void CreateRotatingWheels()
        {
            var rotatingWheelsConfigs = _matchEnvironmentConfigDataService.RotatingWheels;
            if (rotatingWheelsConfigs.IsNullOrEmpty())
            {
                return;
            }
            
            var calculationTick = 0;
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
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, wallConfig.Position, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        var wallId = wallConfig.Id;
                        AddWallToEnvironment(wallId, wallConfig.Points, wallConfig.Position, worldPosition, worldRotation);
                        rotatingWheel.AddWall(wallId);
                    }
                }

                if (!wheelConfig.LavaWalls.IsNullOrEmpty())
                {
                    foreach (var lavaWallConfig in wheelConfig.LavaWalls)
                    {
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, lavaWallConfig.Position, 0,
                            out var worldPosition, out var worldRotation
                        );

                        var lavaWallId = lavaWallConfig.Id;
                        AddLavaWallToEnvironment(lavaWallId, lavaWallConfig.Points, lavaWallConfig.Position, worldPosition, worldRotation);
                        rotatingWheel.AddLavaWall(lavaWallId);
                    }
                }

                if (!wheelConfig.Springs.IsNullOrEmpty())
                {
                    foreach (var springConfig in wheelConfig.Springs)
                    {
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, springConfig.Position, springConfig.RotationAngle,
                            out var worldPosition, out var worldRotation);

                        var springId = springConfig.Id;
                        AddSpringToEnvironment(springId, springConfig.Position, worldPosition, springConfig.RotationAngle, worldRotation);
                        rotatingWheel.AddSpring(springId);
                    }
                }

                if (!wheelConfig.TeleportGatePairs.IsNullOrEmpty())
                {
                    foreach (var teleportPairConfig in wheelConfig.TeleportGatePairs)
                    {
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, teleportPairConfig.GateA.Position, teleportPairConfig.GateA.NormalRotation,
                            out var worldPositionA, out var worldRotationA);

                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, teleportPairConfig.GateB.Position, teleportPairConfig.GateB.NormalRotation,
                            out var worldPositionB, out var worldRotationB);

                        var pairId = teleportPairConfig.Id;
                        AddTeleportGatePairToEnvironment(pairId,
                            teleportPairConfig.GateAId,
                            teleportPairConfig.GateBId,
                            teleportPairConfig.GateA.Position,
                            teleportPairConfig.GateA.NormalRotation,
                            teleportPairConfig.GateB.Position,
                            teleportPairConfig.GateB.NormalRotation,
                            worldPositionA,
                            worldRotationA,
                            worldPositionB,
                            worldRotationB);
                        rotatingWheel.AddTeleportGatePair(pairId);
                    }
                }
            }
        }
    }
}
