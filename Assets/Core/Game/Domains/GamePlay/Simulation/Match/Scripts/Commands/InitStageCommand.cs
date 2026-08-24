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
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions.Linq;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class InitStageCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private IStageDataService _stageDataService;
        private IBonusStageRotationService _bonusStageRotationService;
        private IPlayersPassedScoreGateTrackerService _playersPassedScoreGateTrackerService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private ITeleportGateService _teleportGateService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private NetworkConfig _networkConfig;
        private IMatchEnvironmentConfigDataService _matchEnvironmentConfigDataService;
        private IMolesSpawnCooldownService _molesSpawnCooldownService;
        private IGoldenMoleSpawnedTrackerService _goldenMoleSpawnedTrackerService;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        private IPlayersTalentsManager _playersTalentsManager;
        private IPlayersPowerUpsManager _playersPowerUpsManager;
        private IPowerUpsSpawnerService _powerUpsSpawnerService;
        private IFrigidBlocksController _frigidBlocksController;
        private ICommandFactory _commandFactory;
        private SetRandomTalentsForPlayerCommand _setRandomTalentsForPlayerCommand;
        private TryAddARandomTalentForPlayerCommand _tryAddARandomTalentForPlayerCommand;
        private IPlayersOutsideStageTrackerService _playersOutsideStageTrackerService;
        private IPlayersTouchingWallDataService _playersTouchingWallDataService;
        private IPlayersTouchingSpikesTrackerService _playersTouchingSpikesTrackerService;
        private ILockOnTargetTimerService _lockOnTargetTimerService;
        private List<ushort> _cachedShuffledTeamIds;
        private ITickService _tickService;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _bonusStageRotationService = _diContainer.Resolve<IBonusStageRotationService>();
            _playersPassedScoreGateTrackerService = _diContainer.Resolve<IPlayersPassedScoreGateTrackerService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _teleportGateService = _diContainer.Resolve<ITeleportGateService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _matchEnvironmentConfigDataService = _diContainer.Resolve<IMatchEnvironmentConfigDataService>();
            _molesSpawnCooldownService = _diContainer.Resolve<IMolesSpawnCooldownService>();
            _goldenMoleSpawnedTrackerService = _diContainer.Resolve<IGoldenMoleSpawnedTrackerService>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _playersPowerUpsManager = _diContainer.Resolve<IPlayersPowerUpsManager>();
            _powerUpsSpawnerService = _diContainer.Resolve<IPowerUpsSpawnerService>();
            _frigidBlocksController = _diContainer.Resolve<IFrigidBlocksController>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _setRandomTalentsForPlayerCommand = _commandFactory.CreateCommandVoid<SetRandomTalentsForPlayerCommand>();
            _tryAddARandomTalentForPlayerCommand = _commandFactory.CreateCommandVoid<TryAddARandomTalentForPlayerCommand>();
            _playersOutsideStageTrackerService = _diContainer.Resolve<IPlayersOutsideStageTrackerService>();
            _playersTouchingWallDataService = _diContainer.Resolve<IPlayersTouchingWallDataService>();
            _playersTouchingSpikesTrackerService = _diContainer.Resolve<IPlayersTouchingSpikesTrackerService>();
            _lockOnTargetTimerService = _diContainer.Resolve<ILockOnTargetTimerService>();
            _tickService = _diContainer.Resolve<ITickService>();
            _cachedShuffledTeamIds = new List<ushort>(_sharedGamePlayConfig.MaxTeamsAmount);
        }

        public void Execute()
        {
            LogService.LogTopic("init stage on server side", LogTopicType.ClientNetwork);
            RestartStageData();
            _stageDataService.IncrementStagesEnteredAmount();
            var stageType = ResolveStageTypeForStageNumber(_stageDataService.AmountOfStagesEntered);
            _matchDataService.SimulationState.StageType = stageType;
            SetupBonusStageData(stageType);
            var mapSizeMultiplier = _matchDataService.SimulationState.MapSizeMultiplier = _gamePlayConfigService.GamePlayConfig.StageSizeMultiplier;
            CreateEnvironmentLayout(stageType, mapSizeMultiplier);
            SetupPlayers(mapSizeMultiplier);
        }
        
        private StageType ResolveStageTypeForStageNumber(int stageNumber)
        {
            var gamePlayConfig = _gamePlayConfigService.GamePlayConfig;
            var isRotationConfigured = gamePlayConfig.AreBonusStagesEnabled && gamePlayConfig.BonusStageEveryXStages > 0;
            var didReachBonusStage = isRotationConfigured && stageNumber % gamePlayConfig.BonusStageEveryXStages == 0;
            return didReachBonusStage ? _bonusStageRotationService.ResolveNextBonusStageType() : StageType.DeathMatch;
        }
        
        private void SetupBonusStageData(StageType stageType)
        {
            var simulationState = _matchDataService.SimulationState;

            if (!stageType.IsBonusStage())
            {
                simulationState.WhacAMoleEndTick = 0;
                return;
            }

            var gamePlayConfig = _gamePlayConfigService.GamePlayConfig;
            var bonusStageDurationSeconds = stageType == StageType.WhacAMole
                ? gamePlayConfig.WhacAMole.StageDurationSeconds
                : gamePlayConfig.GatePass.StageDurationSeconds;
            var stageDurationSeconds = gamePlayConfig.PreparationPhaseDuration + bonusStageDurationSeconds;
            var stageDurationTicks = (int)System.MathF.Ceiling(stageDurationSeconds * _networkConfig.TicksPerSeconds);
            simulationState.WhacAMoleEndTick = _tickService.CurrentTick + stageDurationTicks;
        }

        private void CreateEnvironmentLayout(StageType stageType, float mapSizeMultiplier)
        {
            var environmentLayoutId = GenerateNextStageEnvironmentLayoutId(stageType);
            _matchDataService.SimulationState.EnvironmentLayoutId = environmentLayoutId;
            _matchEnvironmentConfigDataService.InitEnvironmentLayout(environmentLayoutId);
            _molesSpawnCooldownService.ClearAllCooldowns();
            _goldenMoleSpawnedTrackerService.ResetGoldenMoleSpawnCounter();

            CreateWalls(mapSizeMultiplier);
            CreateScoreGates(mapSizeMultiplier);
            CreateLavaWalls(mapSizeMultiplier);
            CreateStageBoundaries(mapSizeMultiplier);
            CreateTalentCards(mapSizeMultiplier);
            CreateEnvironmentSprings(mapSizeMultiplier);
            CreateEnvironmentSpikes(mapSizeMultiplier);
            CreateRotatingWheels(mapSizeMultiplier);
            CreateTeleportGates(mapSizeMultiplier);
            CreateGateTraps(mapSizeMultiplier); // after the wheels, so a trap can register its wall with the wheel it rides
            CreateFieldBarriers(mapSizeMultiplier);
        }
        
        private int GenerateNextStageEnvironmentLayoutId(StageType stageType)
        {
            var environmentLayoutId = GetDefaultEnvironmentLayoutId(stageType);
            if (_gamePlayConfigService.GamePlayConfig.ShouldChooseRandomStage)
            {
                environmentLayoutId = GenerateRandomStageId(stageType);
            }

            return environmentLayoutId;
        }

        // Each stage type has its own default layout, because a WhacAMole layout authors mole spawn points
        // that a DeathMatch layout does not, and vice versa.
        private int GetDefaultEnvironmentLayoutId(StageType stageType)
        {
            var gamePlayConfig = _gamePlayConfigService.GamePlayConfig;

            switch (stageType)
            {
                case StageType.WhacAMole: return gamePlayConfig.DefaultWhacAMoleEnvironmentId;
                case StageType.GatePass: return gamePlayConfig.DefaultGatePassEnvironmentId;
                default: return gamePlayConfig.DeafultEnvironmentId;
            }
        }

        // Each stage type draws from its own pool and never repeats a layout until that pool is exhausted.
        private int GenerateRandomStageId(StageType stageType)
        {
            var availableLayoutIndexes = _sharedGamePlayConfig.Environment.GetLayoutIndexesForStageType(stageType);

            if (availableLayoutIndexes.IsNullOrEmpty())
            {
                LogService.LogError($"No environment layout indexes configured for stage type {stageType}!");
                return GetDefaultEnvironmentLayoutId(stageType);
            }

            var didntPlayYetStageIndexes = _matchDataService.GetDidntPlayYetStageIndexes(stageType);

            if (didntPlayYetStageIndexes.IsNullOrEmpty())
            {
                foreach (int index in availableLayoutIndexes)
                {
                    didntPlayYetStageIndexes.Add(index);
                }
            }

            var randomIndex = RNG.NextInt(0, didntPlayYetStageIndexes.Count);
            var environmentLayoutId = didntPlayYetStageIndexes[randomIndex];
            didntPlayYetStageIndexes.RemoveAt(randomIndex);

            return environmentLayoutId;
        }

        private void RestartStageData()
        {
            _physicsSimulator.ClearAllData();
            _playersInLavaTrackerService.ClearAllData();
            _teleportGateService.ClearData();
            ClearStageObjectsInSimulationState();
            var simulationState = _matchDataService.SimulationState;
            simulationState.IsInPreparationPhase = true;
            simulationState.PreperationPhaseStartedOnTick = _tickService.CurrentTick;
            simulationState.PreperationPhaseEndedOnTick = 0;
            simulationState.IsInShowoffWinners = false;
            simulationState.CurrentStageWinnerTeamId = 0;
            simulationState.ResetStageScorePerTeam();
            simulationState.ResetStageScoreForAllPlayers();
            _playersTalentsManager.ResetAllTalentsData();
            _frigidBlocksController.ResetData();
            _playersPowerUpsManager.RemoveAllPowerUps();
            _powerUpsSpawnerService.RestartSpawnTimer();
            _preparationPhaseTimerService.RestartTimer();
            _playersOutsideStageTrackerService.ClearAllData();
            _playersTouchingWallDataService.ClearAllData();
            _playersTouchingSpikesTrackerService.ClearAllData();
            _lockOnTargetTimerService.ResetAllTimers();
            _playersPassedScoreGateTrackerService.ClearAllData(); // stale previous positions across a stage boundary would score phantom passes
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
                player.Spaceship.Transform.AngularVelocity = 0;
                player.Spaceship.Transform.Radius = radius;
                player.Spaceship.IsEngineOn = true;
                player.Spaceship.IsAlive = true;
                player.Spaceship.IsSpinned = false;
                player.Spaceship.IsExposedToLava = false;
                player.Spaceship.AssistArrowType = PlayerAssistArrowType.Hidden;
                player.Spaceship.LockOnTargetObjects.Clear();
                
                if (_gamePlayConfigService.GamePlayConfig.ShouldChooseRandomTalentsForPlayer)
                {
                    _setRandomTalentsForPlayerCommand.SetPlayerId(player.Id).SetTalentsAmount(_gamePlayConfigService.GamePlayConfig.RandomTalentsForPlayersAmount).Execute();
                }
                else if (_gamePlayConfigService.GamePlayConfig.ShouldAddTalentEveryXStages)
                {
                    var didReachStage = _stageDataService.AmountOfStagesEntered % _gamePlayConfigService.GamePlayConfig.EveryXStages == 0;
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
                    talentState.IsCurrentlyActive = false;
                    talentState.IsCurrentlyAiming = false;
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
                ref var refTeamId = ref _matchDataService.SimulationState.FieldBarriersOrderedByTeamId.AddAndGet();
                refTeamId = teamId;
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
            if (wallConfigs.IsNullOrEmpty())
            {
                return;
            }
            
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

        // Only GatePass layouts author score gates, so this is a no-op for every other stage type.
        private void CreateScoreGates(float mapSizeMultiplier)
        {
            var scoreGateConfigs = _matchEnvironmentConfigDataService.ScoreGates;
            if (scoreGateConfigs.IsNullOrEmpty())
            {
                return;
            }

            var postSize = _sharedGamePlayConfig.ScoreGatePostSize.ToNumericsVector2() * mapSizeMultiplier;
            var gapWidth = _sharedGamePlayConfig.ScoreGateGapWidth * mapSizeMultiplier;
            var gatePassConfig = _gamePlayConfigService.GamePlayConfig.GatePass;

            foreach (var scoreGateConfig in scoreGateConfigs)
            {
                var position = scoreGateConfig.Position * mapSizeMultiplier;
                _matchDataService.AddScoreGate(scoreGateConfig.Id, position, scoreGateConfig.RotationDegrees);
                _physicsSimulator.AddScoreGate(scoreGateConfig.Id, position, scoreGateConfig.RotationDegrees, postSize, gapWidth,
                    gatePassConfig.ScoreGateMass, gatePassConfig.ScoreGateDensity, gatePassConfig.ScoreGateRestitution,
                    gatePassConfig.ScoreGateLinearDamping, gatePassConfig.ScoreGateAngularDamping);
            }
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
            var deltaTime = _networkConfig.DeltaTime;
            var calculationTick = 0;

            foreach (var pairConfig in teleportGatePairConfigs)
            {
                TryAttachGateToRotatingWheel(pairConfig.Id, pairConfig.GateA, true, mapSizeMultiplier, calculationTick, deltaTime, rotatingWheelsConfigs, out var worldPosA, out var worldRotA);
                TryAttachGateToRotatingWheel(pairConfig.Id, pairConfig.GateB, false, mapSizeMultiplier, calculationTick, deltaTime, rotatingWheelsConfigs, out var worldPosB, out var worldRotB);

                var scaledGateAPos = pairConfig.GateA.Position * mapSizeMultiplier;
                var scaledGateBPos = pairConfig.GateB.Position * mapSizeMultiplier;

                AddTeleportGatePairToEnvironment(
                    pairConfig.Id,
                    pairConfig.GateAId, pairConfig.GateBId,
                    scaledGateAPos, pairConfig.GateA.NormalRotation,
                    scaledGateBPos, pairConfig.GateB.NormalRotation,
                    worldPosA, worldRotA,
                    worldPosB, worldRotB,
                    mapSizeMultiplier
                );
            }
        }

        private void TryAttachGateToRotatingWheel(
            ushort pairId,
            EnvironmentTeleportGateConfig gateConfig,
            bool isGateA,
            float mapSizeMultiplier,
            int calculationTick,
            float deltaTime,
            EnvironmentRotatingWheelConfig[] rotatingWheelsConfigs,
            out Vector2 worldPosition,
            out float worldRotation)
        {
            var scaledPosition = gateConfig.Position * mapSizeMultiplier;

            if (gateConfig.IsAttachedToRotationWheel)
            {
                var wheel = rotatingWheelsConfigs.FindWithId(gateConfig.AttachToRotationWheelId);

                EnvironmentRotatingWheelUtils.CalculateChildTransform(
                    calculationTick, wheel.RotationSpeed, deltaTime, wheel.CenterPosition * mapSizeMultiplier, scaledPosition, gateConfig.NormalRotation,
                    out worldPosition, out worldRotation
                );

                var rotatingWheel = _matchDataService.EnvironmentData.GetRotatingWheel(wheel.Id);
                rotatingWheel.AddTeleportGatePair(new RotatingTeleportGate(pairId, isGateA));
            }
            else
            {
                worldPosition = scaledPosition;
                worldRotation = gateConfig.NormalRotation;
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

        // A gate trap owns one regular environment wall and drives its transform, so the wall collides like every other
        // wall and the trap only has to decide where it should be this tick.
        private void CreateGateTraps(float mapSizeMultiplier)
        {
            var gateTrapConfigs = _matchEnvironmentConfigDataService.GateTraps;
            if (gateTrapConfigs.IsNullOrEmpty())
            {
                return;
            }

            // Runs after every other wall layer, so this sees the whole layout's wall ids - including earlier traps'.
            foreach (var gateTrapConfig in gateTrapConfigs)
            {
                if (_matchDataService.EnvironmentData.TryGetEnvironmentWall(gateTrapConfig.WallId, out _))
                {
                    // Two walls sharing an id makes CopyWallStateToBody drive both bodies onto one transform, dragging
                    // an authored wall around with the trap, so the trap is dropped instead of breaking the arena.
                    LogService.LogError($"Gate trap {gateTrapConfig.Id} reuses wall id {gateTrapConfig.WallId}, which the layout already owns! Skipping this gate trap.");
                    continue;
                }

                var gateTrap = AddGateTrapToEnvironment(gateTrapConfig, mapSizeMultiplier);
                AddGateTrapWallToEnvironment(gateTrap, gateTrapConfig, mapSizeMultiplier);

                ref var gateTrapState = ref _matchDataService.SimulationState.GateTraps.AddAndGet();
                gateTrapState.Id = gateTrap.Id;
                gateTrapState.State = GateTrapState.Open;
                gateTrapState.StateEndTick = 0; // a fresh stage arms every trap immediately
            }
        }

        private MatchEnvironmentGateTrapModel AddGateTrapToEnvironment(EnvironmentGateTrapConfig gateTrapConfig, float mapSizeMultiplier)
        {
            var ticksPerSecond = _networkConfig.TicksPerSeconds;
            var gateTrap = _matchDataService.EnvironmentData.AddGateTrap(gateTrapConfig.Id, gateTrapConfig.WallId);

            gateTrap.OpenPosition = gateTrapConfig.OpenPosition * mapSizeMultiplier;
            gateTrap.ClosedPosition = gateTrapConfig.ClosedPosition * mapSizeMultiplier;
            gateTrap.OpenRotationDegrees = gateTrapConfig.OpenRotationDegrees;
            gateTrap.ClosedRotationDegrees = gateTrapConfig.ClosedRotationDegrees;
            gateTrap.LocalRotationPivot = gateTrapConfig.LocalRotationPivot * mapSizeMultiplier;
            gateTrap.IsAttachedToRotationWheel = gateTrapConfig.IsAttachedToRotationWheel;
            gateTrap.AttachedToRotationWheelId = gateTrapConfig.AttachToRotationWheelId;
            gateTrap.AreaPolygons = ScaleGateTrapAreaPolygons(gateTrapConfig, mapSizeMultiplier);
            // The travelled distance scales with the map, so the speed scales with it too and the cycle keeps its authored timing.
            gateTrap.TransitionDurationInTicks = EnvironmentGateTrapUtils.CalculateTransitionDurationInTicks(gateTrap.OpenPosition, gateTrap.ClosedPosition,
                gateTrap.OpenRotationDegrees, gateTrap.ClosedRotationDegrees, gateTrapConfig.MovementSpeed * mapSizeMultiplier, ticksPerSecond);
            gateTrap.StayClosedDurationInTicks = EnvironmentGateTrapUtils.SecondsToTicks(gateTrapConfig.SecondsStayClosed, ticksPerSecond);
            gateTrap.StayOpenDurationInTicks = EnvironmentGateTrapUtils.SecondsToTicks(gateTrapConfig.SecondsStayOpen, ticksPerSecond);

            return gateTrap;
        }

        private void AddGateTrapWallToEnvironment(MatchEnvironmentGateTrapModel gateTrap, EnvironmentGateTrapConfig gateTrapConfig, float mapSizeMultiplier)
        {
            var calculationTick = 0;

            EnvironmentGateTrapUtils.CalculateWallTransform(gateTrap.OpenPosition, gateTrap.ClosedPosition, gateTrap.OpenRotationDegrees, gateTrap.ClosedRotationDegrees,
                gateTrap.LocalRotationPivot, 0f, out var localPosition, out var localRotation);

            var worldPosition = localPosition;
            var worldRotation = localRotation;

            if (gateTrap.IsAttachedToRotationWheel)
            {
                var wheelConfig = _matchEnvironmentConfigDataService.RotatingWheels.FindWithId(gateTrap.AttachedToRotationWheelId);
                EnvironmentRotatingWheelUtils.CalculateChildTransform(
                    calculationTick, wheelConfig.RotationSpeed, _networkConfig.DeltaTime, wheelConfig.CenterPosition * mapSizeMultiplier, localPosition, localRotation,
                    out worldPosition, out worldRotation
                );

                _matchDataService.EnvironmentData.GetRotatingWheel(gateTrap.AttachedToRotationWheelId).AddWall(gateTrap.WallId);
            }

            var wallPoints = new Vector2[gateTrapConfig.WallPoints.Length];
            for (int i = 0; i < wallPoints.Length; i++)
            {
                wallPoints[i] = gateTrapConfig.WallPoints[i] * mapSizeMultiplier;
            }

            AddWallToEnvironment(gateTrap.WallId, wallPoints, localPosition, worldPosition, worldRotation);
            // Unlike a wheel's own walls the trap wall swings, so its local rotation is not zero and the wheel step needs it.
            _matchDataService.EnvironmentData.GetWall(gateTrap.WallId).Transform.LocalRotationDegrees = localRotation;
        }

        private Vector2[][] ScaleGateTrapAreaPolygons(EnvironmentGateTrapConfig gateTrapConfig, float mapSizeMultiplier)
        {
            if (gateTrapConfig.AreaPolygons.IsNullOrEmpty())
            {
                LogService.LogError($"Gate trap {gateTrapConfig.Id} has no area polygons, it will never close!");
                return System.Array.Empty<Vector2[]>();
            }

            var areaPolygons = new Vector2[gateTrapConfig.AreaPolygons.Length][];

            for (int polygonIndex = 0; polygonIndex < areaPolygons.Length; polygonIndex++)
            {
                var polygonPoints = gateTrapConfig.AreaPolygons[polygonIndex].Points;
                var scaledPoints = new Vector2[polygonPoints.Length];

                for (int pointIndex = 0; pointIndex < polygonPoints.Length; pointIndex++)
                {
                    scaledPoints[pointIndex] = polygonPoints[pointIndex] * mapSizeMultiplier;
                }

                areaPolygons[polygonIndex] = scaledPoints;
            }

            return areaPolygons;
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
