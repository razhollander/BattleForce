using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TrySpawnMolesCommand : BaseCommand, ICommandVoid
    {
        private const ushort NO_MOLE_HOLE_ID = 0;

        private static readonly PhysicsBodyType[] BLOCKING_SPAWN_BODY_TYPES =
        {
            PhysicsBodyType.Wall, PhysicsBodyType.Lava, PhysicsBodyType.StartMatchWall, PhysicsBodyType.Mole, PhysicsBodyType.PlayerSpaceship,
        };

        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private IMolesSpawnTimerService _molesSpawnTimerService;
        private IGoldenMoleSpawnedTrackerService _goldenMoleSpawnedTrackerService;
        private IMolesSpawnCooldownService _molesSpawnCooldownService;
        private IPhysicsSimulator _physicsSimulator;
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private IMatchEnvironmentConfigDataService _matchEnvironmentConfigDataService;
        private IStageDataService _stageDataService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private NetworkConfig _networkConfig;

        private int _processedTick;

        public TrySpawnMolesCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _molesSpawnTimerService = _diContainer.Resolve<IMolesSpawnTimerService>();
            _goldenMoleSpawnedTrackerService = _diContainer.Resolve<IGoldenMoleSpawnedTrackerService>();
            _molesSpawnCooldownService = _diContainer.Resolve<IMolesSpawnCooldownService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _matchEnvironmentConfigDataService = _diContainer.Resolve<IMatchEnvironmentConfigDataService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;

            if (simulationState.StageType != StageType.WhacAMole)
            {
                return;
            }

            var isStageAcceptingMoles = !simulationState.IsInPreparationPhase && !_stageDataService.IsStageEnded && _processedTick < simulationState.WhacAMoleEndTick;
            if (!isStageAcceptingMoles)
            {
                return;
            }

            var isSpawnTimerEnded = _molesSpawnTimerService.IsSpawnTimerEnded();
            if (isSpawnTimerEnded)
            {
                _molesSpawnTimerService.RestartSpawnTimer();
            }

            var whacAMoleConfig = _gamePlayConfigService.GamePlayConfig.WhacAMole;
            var areCurrentlyMaxMoles = simulationState.Moles.Count >= whacAMoleConfig.MaxConcurrentMoles;

            if (isSpawnTimerEnded && !areCurrentlyMaxMoles)
            {
                SpawnMole(whacAMoleConfig);
            }
        }
        
        private void SpawnMole(WhacAMoleConfig whacAMoleConfig)
        {
            if (!TryFindAvailableSpawnPoint(whacAMoleConfig.MoleRadius, out var moleHoleId, out var position))
            {
                return;
            }

            var isGolden = _goldenMoleSpawnedTrackerService.ShouldSpawnGoldenMole();
            var lives = (byte)(isGolden ? whacAMoleConfig.GoldenMoleLives : 1);
            var emergeOnTick = _processedTick + CalculateHoleShakeTicks();
            var mole = _matchDataService.AddMole(moleHoleId, position, emergeOnTick, CalculateDisappearOnTick(whacAMoleConfig, emergeOnTick), isGolden, lives);
            _goldenMoleSpawnedTrackerService.RegisterMoleSpawned(isGolden);
            _netEventsDataService.AddMoleSpawnedNetEvent(_processedTick, mole.Id, moleHoleId, emergeOnTick, isGolden, lives);
        }

        private int CalculateHoleShakeTicks()
        {
            return (int)System.MathF.Ceiling(_sharedGamePlayConfig.MoleHoleShakeDurationSeconds * _networkConfig.TicksPerSeconds);
        }
        
        private int CalculateDisappearOnTick(WhacAMoleConfig whacAMoleConfig, int emergeOnTick)
        {
            if (whacAMoleConfig.MaxMoleLifetimeSeconds <= 0)
            {
                return MoleStateS2C.NEVER_EXPIRES_TICK;
            }

            var lifetimeSeconds = RNG.NextFloat(whacAMoleConfig.MinMoleLifetimeSeconds, whacAMoleConfig.MaxMoleLifetimeSeconds);
            var lifetimeTicks = (int)System.MathF.Ceiling(lifetimeSeconds * _networkConfig.TicksPerSeconds);
            return emergeOnTick + lifetimeTicks;
        }

        private bool TryFindAvailableSpawnPoint(float moleRadius, out ushort moleHoleId, out Vector2 position)
        {
            moleHoleId = NO_MOLE_HOLE_ID;
            position = Vector2.Zero;
            var spawnPoints = _matchEnvironmentConfigDataService.MoleSpawnPoints;

            if (spawnPoints.IsNullOrEmpty())
            {
                LogService.LogError($"No mole spawn points authored for environment layout {_matchDataService.SimulationState.EnvironmentLayoutId}!");
                return false;
            }

            var mapSizeMultiplier = _matchDataService.SimulationState.MapSizeMultiplier;
            var startIndex = RNG.NextInt(spawnPoints.Length);

            for (var i = 0; i < spawnPoints.Length; i++)
            {
                var spawnPoint = spawnPoints[(startIndex + i) % spawnPoints.Length];
                var candidatePosition = spawnPoint.Position * mapSizeMultiplier;
                var isSpawnPointFree = !_molesSpawnCooldownService.IsMoleHoleOnCooldown(spawnPoint.MoleHoleId, _processedTick)
                                       && !_physicsSimulator.IsSquareHitAnyBodyTypes(candidatePosition, moleRadius, BLOCKING_SPAWN_BODY_TYPES)
                                       && !IsSpawnPointTakenByShakingMole(spawnPoint.MoleHoleId);

                if (isSpawnPointFree)
                {
                    moleHoleId = spawnPoint.MoleHoleId;
                    position = candidatePosition;
                    return true;
                }
            }

            return false;
        }
        
        private bool IsSpawnPointTakenByShakingMole(ushort moleHoleId)
        {
            foreach (var mole in _matchDataService.SimulationState.Moles.AsSpan())
            {
                if (!mole.IsEmerged && mole.MoleHoleId == moleHoleId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
