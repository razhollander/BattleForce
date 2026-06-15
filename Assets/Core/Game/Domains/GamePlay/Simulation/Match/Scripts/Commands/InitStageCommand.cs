using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
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
        private List<ushort> _cachedShuffledTeamIds;
        
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
            _cachedShuffledTeamIds = new List<ushort>(_sharedGamePlayConfig.MaxTeamsAmount);
        }

        public void Execute()
        {
            LogService.LogError("init stage on server side");
            ClearStageData();
            var mapSizeMultiplier = _matchDataService.SimulationState.MapSizeMultiplier = _gamePlayConfigService.GamePlayConfig.StageSizeMultiplier;
            CreateEnvironmentLayout(mapSizeMultiplier);
            SetupPlayers(mapSizeMultiplier);
            _stageNumber++;
        }

        private void CreateEnvironmentLayout(float mapSizeMultiplier)
        {
            var environmentLayoutId = GenerateNextStageEnvironmentLayoutId();
            _matchDataService.SimulationState.EnvironmentLayoutId = environmentLayoutId;
            _matchEnvironmentConfigDataService.InitEnvironmentLayout(environmentLayoutId);
            
            CreateWalls(mapSizeMultiplier);
            CreateLavaWalls(mapSizeMultiplier);
            CreateStageBoundaries(mapSizeMultiplier);
            CreateTalentCards(mapSizeMultiplier);
            CreateEnvironmentSprings(mapSizeMultiplier);
            CreateEnvironmentSpikes(mapSizeMultiplier);
            CreateRotatingWheels(mapSizeMultiplier);
            CreateTeleportGates(mapSizeMultiplier);
            CreateFieldBarriers(mapSizeMultiplier);
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

        private void SetupPlayers(float mapSizeMultiplier)
        {
            var players = _matchDataService.SimulationState.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players.GetByIndex(i);

                var health = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.StartHealth;
                var shootCooldown = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.ShootCooldown;
                var radius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
                var heartRadius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.DefaultHeartRadius;

                var teamId = player.TeamId;

                var barrier = GetBarrierForTeam(teamId);
                var position = barrier.Position * mapSizeMultiplier;

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

        private void CreateFieldBarriers(float mapSizeMultiplier)
        {
            var barrierConfigs = _matchEnvironmentConfigDataService.FieldBarrierConfigs;
            if (barrierConfigs.IsNullOrEmpty())
            {
                return;
            }

            _cachedShuffledTeamIds.Clear();
            _cachedShuffledTeamIds.AddRange(_matchDataService.TeamIds);
            RNG.Shuffle(_cachedShuffledTeamIds);

            int barrierIndex = 0;
            foreach (var teamId in _cachedShuffledTeamIds)
            {
                if (barrierIndex >= barrierConfigs.Length)
                {
                    break;
                }

                var barrierConfig = barrierConfigs[barrierIndex];
                _matchDataService.EnvironmentData.AddFieldBarrier((ushort)barrierIndex, teamId, barrierConfig.Position * mapSizeMultiplier, barrierConfig.Size * mapSizeMultiplier, barrierConfig.Shape);
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

        private void CreateWalls(float mapSizeMultiplier)
        {
            var wallConfigs = _matchEnvironmentConfigDataService.WallConfigs;

            foreach (var wallConfig in wallConfigs)
            {
                var points = new Vector2[wallConfig.Points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = wallConfig.Points[i] * mapSizeMultiplier;
                }
                AddWallToEnvironment(wallConfig.Id, points, wallConfig.Position * mapSizeMultiplier, wallConfig.Position * mapSizeMultiplier, 0);
            }
        }

        private void AddWallToEnvironment(ushort wallId, Vector2[] wallPoints, Vector2 lavaWallLocalPosition, Vector2 lavaWallWorldPosition, float lavaWallWorldRotationAngle)
        {
            _matchDataService.EnvironmentData.AddWall(wallId, wallPoints, lavaWallLocalPosition, lavaWallWorldPosition, lavaWallWorldRotationAngle);
            _physicsSimulator.AddWall(wallId, wallPoints, lavaWallWorldPosition);
        }

        private void CreateLavaWalls(float mapSizeMultiplier)
        {
            var lavaWallConfigs = _matchEnvironmentConfigDataService.LavaWallConfigs;
            if (lavaWallConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var lavaWallConfig in lavaWallConfigs)
            {
                var points = new Vector2[lavaWallConfig.Points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = lavaWallConfig.Points[i] * mapSizeMultiplier;
                }
                AddLavaWallToEnvironment(lavaWallConfig.Id, points, lavaWallConfig.Position * mapSizeMultiplier, lavaWallConfig.Position * mapSizeMultiplier, 0);
            }
        }

        private void AddLavaWallToEnvironment(ushort lavaWallId, Vector2[] lavaWallPoints, Vector2 lavaWallLocalPosition, Vector2 lavaWallWorldPosition, float lavaWallWorldRotationAngle)
        {
            _matchDataService.EnvironmentData.AddLavaWall(lavaWallId, lavaWallPoints, lavaWallLocalPosition, lavaWallWorldPosition, lavaWallWorldRotationAngle);
            _physicsSimulator.AddLavaWall(lavaWallId, lavaWallPoints, lavaWallWorldPosition);
        }

        private void CreateStageBoundaries(float mapSizeMultiplier)
        {
            var stageBoundaryConfigs = _matchEnvironmentConfigDataService.StageBoundaries;
            if (stageBoundaryConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var boundaryConfig in stageBoundaryConfigs)
            {
                var points = new Vector2[boundaryConfig.Points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = boundaryConfig.Points[i] * mapSizeMultiplier;
                }
                AddStageBoundaryToEnvironment(boundaryConfig.Id, points, boundaryConfig.Position * mapSizeMultiplier, boundaryConfig.Position * mapSizeMultiplier, 0);
            }
        }

        private void AddStageBoundaryToEnvironment(ushort stageBoundaryId, Vector2[] stageBoundaryPoints, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle)
        {
            _matchDataService.EnvironmentData.AddStageBoundary(stageBoundaryId, stageBoundaryPoints, localPosition, worldPosition, worldRotationAngle);
            _physicsSimulator.AddStageBoundary(stageBoundaryId, stageBoundaryPoints, worldPosition);
        }

        private void CreateTalentCards(float mapSizeMultiplier)
        {
            var talentCards = _matchEnvironmentConfigDataService.TalentCards;
            if (talentCards.IsNullOrEmpty())
            {
                return;
            }

            foreach (var talentCard in talentCards)
            {
                var talentCardPosition = talentCard.Position * mapSizeMultiplier;
                var talentCardId = talentCard.Id;
                _matchDataService.AddTalentCard(talentCardId, talentCardPosition, talentCard.TalentType, _gamePlayConfigService.GamePlayConfig.Talents.TalentCardHealth);
                _physicsSimulator.AddTalentCard(talentCardId, talentCardPosition, _gamePlayConfigService.GamePlayConfig.Talents.TalentCardWidth, _gamePlayConfigService.GamePlayConfig.Talents.TalentCardHeight);
            }
        }

        private void CreateEnvironmentSprings(float mapSizeMultiplier)
        {
            var environmentSprings = _matchEnvironmentConfigDataService.EnvironmentSprings;
            if (environmentSprings.IsNullOrEmpty())
            {
                return;
            }

            foreach (var environmentSpring in environmentSprings)
            {
                AddSpringToEnvironment(environmentSpring.Id, Vector2.Zero, environmentSpring.Position * mapSizeMultiplier, 0, environmentSpring.RotationAngle);
            }
        }

        private void AddSpringToEnvironment(ushort springId, Vector2 springLocalPosition, Vector2 springWorldPosition, float springLocalRotationAngle, float springWorldRotationAngle)
        {
            var springSize = _gamePlayConfigService.GamePlayConfig.EnvironmentSprings.Size.ToNumericsVector2();
            _matchDataService.EnvironmentData.AddSpring(springId, springLocalPosition, springWorldPosition, springLocalRotationAngle, springWorldRotationAngle);
            _physicsSimulator.AddEnvironmentSpring(springId, springWorldPosition, springWorldRotationAngle, springSize);
        }

        private void CreateEnvironmentSpikes(float mapSizeMultiplier)
        {
            var environmentSpikes = _matchEnvironmentConfigDataService.EnvironmentSpikes;
            if (environmentSpikes.IsNullOrEmpty())
            {
                return;
            }

            foreach (var environmentSpike in environmentSpikes)
            {
                AddSpikeToEnvironment(environmentSpike.Id, Vector2.Zero, environmentSpike.Position * mapSizeMultiplier, 0, environmentSpike.RotationAngle);
            }
        }

        private void AddSpikeToEnvironment(ushort spikeId, Vector2 spikeLocalPosition, Vector2 spikeWorldPosition, float spikeLocalRotationAngle, float spikeWorldRotationAngle)
        {
            var spikeSize = _gamePlayConfigService.GamePlayConfig.EnvironmentSpikes.Size.ToNumericsVector2();
            _matchDataService.EnvironmentData.AddSpike(spikeId, spikeLocalPosition, spikeWorldPosition, spikeLocalRotationAngle, spikeWorldRotationAngle);
            _physicsSimulator.AddEnvironmentSpike(spikeId, spikeWorldPosition, spikeWorldRotationAngle, spikeSize);
        }
        
        private void CreateTeleportGates(float mapSizeMultiplier)
        {
            var teleportGatePairConfigs = _matchEnvironmentConfigDataService.TeleportGates;
            if (teleportGatePairConfigs.IsNullOrEmpty())
            {
                return;
            }

            var rotatingWheelsConfigs = _matchEnvironmentConfigDataService.RotatingWheels;
            var wheelsDict = new System.Collections.Generic.Dictionary<ushort, Core.Game.Domains.GamePlay.Shared.Scripts.Configs.EnvironmentRotatingWheelConfig>();
            if (!rotatingWheelsConfigs.IsNullOrEmpty())
            {
                foreach (var wheel in rotatingWheelsConfigs)
                {
                    wheelsDict[wheel.Id] = wheel;
                }
            }

            var calculationTick = 0;
            var deltaTime = _networkConfig.DeltaTime;

            foreach (var teleportGatePairConfig in teleportGatePairConfigs)
            {
                var gateA = teleportGatePairConfig.GateA;
                var scaledGateAPos = gateA.Position * mapSizeMultiplier;
                Vector2 worldPositionA;
                float worldRotationA;

                if (gateA.IsAttachedToRotationWheel && wheelsDict.TryGetValue(gateA.AttachToRotationWheelId, out var wheelA))
                {
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, wheelA.RotationSpeed, deltaTime, wheelA.CenterPosition * mapSizeMultiplier, scaledGateAPos, gateA.NormalRotation,
                        out worldPositionA, out worldRotationA);

                    var rotatingWheelS2C = _matchDataService.EnvironmentData.GetRotatingWheel(wheelA.Id);
                    rotatingWheelS2C.AddTeleportGatePair(new RotatingTeleportGate(teleportGatePairConfig.Id, true));
                }
                else
                {
                    worldPositionA = scaledGateAPos;
                    worldRotationA = gateA.NormalRotation;
                }

                var gateB = teleportGatePairConfig.GateB;
                var scaledGateBPos = gateB.Position * mapSizeMultiplier;
                Vector2 worldPositionB;
                float worldRotationB;

                if (gateB.IsAttachedToRotationWheel && wheelsDict.TryGetValue(gateB.AttachToRotationWheelId, out var wheelB))
                {
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, wheelB.RotationSpeed, deltaTime, wheelB.CenterPosition * mapSizeMultiplier, scaledGateBPos, gateB.NormalRotation,
                        out worldPositionB, out worldRotationB);

                    var rotatingWheelS2C = _matchDataService.EnvironmentData.GetRotatingWheel(wheelB.Id);
                    rotatingWheelS2C.AddTeleportGatePair(new RotatingTeleportGate(teleportGatePairConfig.Id, false));
                }
                else
                {
                    worldPositionB = scaledGateBPos;
                    worldRotationB = gateB.NormalRotation;
                }

                AddTeleportGatePairToEnvironment(teleportGatePairConfig.Id, teleportGatePairConfig.GateAId, teleportGatePairConfig.GateBId, scaledGateAPos, gateA.NormalRotation, scaledGateBPos,
                    gateB.NormalRotation, worldPositionA, worldRotationA, worldPositionB, worldRotationB, mapSizeMultiplier);
            }
        }
        
        private void AddTeleportGatePairToEnvironment(ushort teleportPairId, ushort gateAId, ushort gateBId, Vector2 gateAPosition, float gateANormalRotation, Vector2 gateBPosition,
            float gateBNormalRotation, Vector2 gateAWorldPosition, float gateAWorldRotation, Vector2 gateBWorldPosition, float gateBWorldRotation, float mapSizeMultiplier)
        {
            var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size.ToNumericsVector2() * mapSizeMultiplier;
            _matchDataService.EnvironmentData.AddTeleportGatePair(teleportPairId, gateAId, gateBId, gateAPosition, gateANormalRotation, gateBPosition, gateBNormalRotation,
                gateAWorldPosition, gateAWorldRotation, gateBWorldPosition, gateBWorldRotation);
            _physicsSimulator.AddTeleportGate(gateAId, gateAWorldPosition, gateAWorldRotation, gateSize);
            _physicsSimulator.AddTeleportGate(gateBId, gateBWorldPosition, gateBWorldRotation, gateSize);
        }

        private void CreateRotatingWheels(float mapSizeMultiplier)
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
                var wheelCenter = wheelConfig.CenterPosition * mapSizeMultiplier;
                var rotationSpeed = wheelConfig.RotationSpeed;
                var rotatingWheel = _matchDataService.EnvironmentData.AddRotatingWheel(wheelConfig.Id, wheelCenter, rotationSpeed);

                if (!wheelConfig.Walls.IsNullOrEmpty())
                {
                    foreach (var wallConfig in wheelConfig.Walls)
                    {
                        var scaledPosition = wallConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        var wallId = wallConfig.Id;
                        var points = new Vector2[wallConfig.Points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = wallConfig.Points[i] * mapSizeMultiplier;
                        }
                        AddWallToEnvironment(wallId, points, scaledPosition, worldPosition, worldRotation);
                        rotatingWheel.AddWall(wallId);
                    }
                }

                if (!wheelConfig.LavaWalls.IsNullOrEmpty())
                {
                    foreach (var lavaWallConfig in wheelConfig.LavaWalls)
                    {
                        var scaledPosition = lavaWallConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, 0,
                            out var worldPosition, out var worldRotation
                        );

                        var lavaWallId = lavaWallConfig.Id;
                        var points = new Vector2[lavaWallConfig.Points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = lavaWallConfig.Points[i] * mapSizeMultiplier;
                        }
                        AddLavaWallToEnvironment(lavaWallId, points, scaledPosition, worldPosition, worldRotation);
                        rotatingWheel.AddLavaWall(lavaWallId);
                    }
                }

                if (!wheelConfig.Springs.IsNullOrEmpty())
                {
                    foreach (var springConfig in wheelConfig.Springs)
                    {
                        var scaledPosition = springConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, springConfig.RotationAngle,
                            out var worldPosition, out var worldRotation);

                        var springId = springConfig.Id;
                        AddSpringToEnvironment(springId, scaledPosition, worldPosition, springConfig.RotationAngle, worldRotation);
                        rotatingWheel.AddSpring(springId);
                    }
                }
                
                if (!wheelConfig.Spikes.IsNullOrEmpty())
                {
                    foreach (var spikeConfig in wheelConfig.Spikes)
                    {
                        var scaledPosition = spikeConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, spikeConfig.RotationAngle,
                            out var worldPosition, out var worldRotation);

                        var spikeId = spikeConfig.Id;
                        AddSpikeToEnvironment(spikeId, scaledPosition, worldPosition, spikeConfig.RotationAngle, worldRotation);
                        rotatingWheel.AddSpike(spikeId);
                    }
                }

            }
        }
    }
}
