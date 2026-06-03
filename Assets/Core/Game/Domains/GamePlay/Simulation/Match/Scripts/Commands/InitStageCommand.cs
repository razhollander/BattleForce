using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System;
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
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class InitStageCommand : BaseCommand, ICommandVoid
    {
        private static int _stageNumber = 1;
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private IStageDataService _stageDataService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private ITeleportGateService _teleportGateService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private NetworkConfig _networkConfig;
        private IMatchEnvironmentConfigDataService _matchEnvironmentConfigDataService;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        private IPlayersTalentsManager _playersTalentsManager;
        private ICommandFactory _commandFactory;
        private SetRandomTalentsForPlayerCommand _setRandomTalentsForPlayerCommand;
        private TryAddARandomTalentForPlayerCommand _tryAddARandomTalentForPlayerCommand;
        private IPlayersOutsideStageTrackerService _playersOutsideStageTrackerService;
        private ILockOnTargetTimerService _lockOnTargetTimerService;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _teleportGateService = _diContainer.Resolve<ITeleportGateService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _matchEnvironmentConfigDataService = _diContainer.Resolve<IMatchEnvironmentConfigDataService>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _setRandomTalentsForPlayerCommand = _commandFactory.CreateCommandVoid<SetRandomTalentsForPlayerCommand>();
            _tryAddARandomTalentForPlayerCommand = _commandFactory.CreateCommandVoid<TryAddARandomTalentForPlayerCommand>();
            _playersOutsideStageTrackerService = _diContainer.Resolve<IPlayersOutsideStageTrackerService>();
            _lockOnTargetTimerService = _diContainer.Resolve<ILockOnTargetTimerService>();
        }

        public void Execute()
        {
            LogService.LogError("init stage on server side");
            ClearStageData();
            
            CreateEnvironmentLayout();
            SetupPlayers();
            _stageNumber++;
        }

        private void CreateEnvironmentLayout()
        {
            var environmentLayoutId = GenerateNextStageEnvironmentLayoutId();
            _matchDataService.SimulationState.EnvironmentLayoutId = environmentLayoutId;
            _matchEnvironmentConfigDataService.InitEnvironmentLayout(environmentLayoutId);
            
            CreateWalls();
            CreateLavaWalls();
            CreateStageBoundaries();
            CreateTalentCards();
            CreateEnvironmentSprings();
            CreateTeleportGates();
            CreateRotatingWheels();
            CreateFieldBarriers();
        }
        
        private int GenerateNextStageEnvironmentLayoutId()
        {
            var environmentLayoutId = _gamePlayConfigService.GamePlayConfig.DeafultEnvironmentId;
            if (_gamePlayConfigService.GamePlayConfig.ShouldChooseRandomStage)
            {
                environmentLayoutId = GenerateRandomStageId();
            }

            return environmentLayoutId;
        }

        private int GenerateRandomStageId()
        {
            var didntPlayYetStageIndexes = _matchDataService.DidntPlayYetStageIndexes;

            if (didntPlayYetStageIndexes.IsNullOrEmpty())
            {
                foreach (int index in _sharedGamePlayConfig.Environment.AvailableLayoutIndexes)
                {
                    didntPlayYetStageIndexes.Add(index);
                }
            }
                
            var randomIndex = RNG.NextInt(0, didntPlayYetStageIndexes.Count);
            var environmentLayoutId = didntPlayYetStageIndexes[randomIndex];
            didntPlayYetStageIndexes.RemoveAt(randomIndex);

            return environmentLayoutId;
        }

        private void ClearStageData()
        {
            _physicsSimulator.ClearAllData();
            _playersInLavaTrackerService.ClearAllData();
            _teleportGateService.ClearData();
            ClearStageObjectsInSimulationState();
            _matchDataService.SimulationState.IsInPreparationPhase = true;
            _matchDataService.SimulationState.StartPhaseInitialTick = 0;
            _matchDataService.SimulationState.IsInShowoffWinners = false;
            _matchDataService.SimulationState.CurrentStageWinnerTeamId = 0;
            _playersTalentsManager.ResetAllTalentsData();
            _preparationPhaseTimerService.RestartTimer();
            _playersOutsideStageTrackerService.ClearAllData();
            _lockOnTargetTimerService.ResetAllTimers();
            _stageDataService.ClearData();
        }

        private void ClearStageObjectsInSimulationState()
        {
            _matchDataService.SimulationState.ClearObjectStates();
            _matchDataService.EnvironmentData.ClearData();
        }

        private void SetupPlayers()
        {
            var halfSize = _matchEnvironmentConfigDataService.EnvironmentHalfSize;
            var players = _matchDataService.SimulationState.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players.GetByIndex(i);

                var health = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.StartHealth;
                var shootCooldown = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.ShootCooldown;
                var radius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
                var heartRadius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.DefaultHeartRadius;

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
                var velocity = direction * _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.TargetMovementSpeed;

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
                player.Spaceship.IsSpinned = false;
                player.Spaceship.TargetedEnemyIds.Clear();
                
                if (_gamePlayConfigService.GamePlayConfig.ShouldChooseRandomTalentsForPlayer)
                {
                    _setRandomTalentsForPlayerCommand.SetPlayerId(player.Id).SetTalentsAmount(_gamePlayConfigService.GamePlayConfig.RandomTalentsForPlayersAmount).Execute();
                }
                else if (_gamePlayConfigService.GamePlayConfig.ShouldAddTalentEveryXStages)
                {
                    var didReachStage = _stageNumber % _gamePlayConfigService.GamePlayConfig.EveryXStages == 0;
                    if (didReachStage)
                    {
                        _tryAddARandomTalentForPlayerCommand.SetPlayerId(player.Id).Execute();
                    }
                }

                var talentsCount = player.Spaceship.TalentsState.Talents.Count;
                for (var k = 0; k < talentsCount; k++)
                {
                    ref var talentState = ref player.Spaceship.TalentsState.Talents.Get(k);
                    talentState.ClearCooldown();
                }
                
                _physicsSimulator.AddPlayer(player.Id, player.TeamId, position, velocity, radius, heartRadius);
            }
        }

        private MatchEnvironmentFieldBarrierModel GetBarrierForTeam(ushort teamId)
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

                var barrierConfig = barrierConfigs[barrierIndex];
                _matchDataService.EnvironmentData.AddFieldBarrier((ushort)barrierIndex, teamId, barrierConfig.Position, barrierConfig.Size, barrierConfig.Shape);
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

        private void CreateStageBoundaries()
        {
            var stageBoundaryConfigs = _matchEnvironmentConfigDataService.StageBoundaries;
            if (stageBoundaryConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var boundaryConfig in stageBoundaryConfigs)
            {
                AddStageBoundaryToEnvironment(boundaryConfig.Id, boundaryConfig.Points, boundaryConfig.Position, boundaryConfig.Position, 0);
            }
        }

        private void AddStageBoundaryToEnvironment(ushort stageBoundaryId, Vector2[] stageBoundaryPoints, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle)
        {
            _matchDataService.EnvironmentData.AddStageBoundary(stageBoundaryId, stageBoundaryPoints, localPosition, worldPosition, worldRotationAngle);
            _physicsSimulator.AddStageBoundary(stageBoundaryId, stageBoundaryPoints, worldPosition);
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
                _matchDataService.AddTalentCard(talentCardId, talentCardPosition, talentCard.TalentType, _gamePlayConfigService.GamePlayConfig.Talents.TalentCardHealth);
                _physicsSimulator.AddTalentCard(talentCardId, talentCardPosition, _gamePlayConfigService.GamePlayConfig.Talents.TalentCardWidth, _gamePlayConfigService.GamePlayConfig.Talents.TalentCardHeight);
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
            var springSize = _gamePlayConfigService.GamePlayConfig.EnvironmentSprings.Size.ToNumericsVector2();
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
