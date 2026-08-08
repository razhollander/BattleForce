using System.Numerics;
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
        private const int NEVER_EXPIRES_TICK = 0;
        private const int NOT_EXPIRING_TICK = 0; // a mole that has not started its pre-hide shake yet has a zero hide tick

        private static readonly PhysicsBodyType[] BLOCKING_SPAWN_BODY_TYPES =
        {
            PhysicsBodyType.Wall, PhysicsBodyType.Lava, PhysicsBodyType.StartMatchWall, PhysicsBodyType.Mole, PhysicsBodyType.PlayerSpaceship,
        };

        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private IMolesSpawnerService _molesSpawnerService;
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
            _molesSpawnerService = _diContainer.Resolve<IMolesSpawnerService>();
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

            ProcessExpiringMoles();
            EmergeMolesWhoseHoleFinishedShaking();

            var isStageAcceptingMoles = !simulationState.IsInPreparationPhase && !_stageDataService.IsStageEnded;
            if (!isStageAcceptingMoles)
            {
                return;
            }

            var isSpawnTimerEnded = _molesSpawnerService.IsSpawnTimerEnded();
            if (isSpawnTimerEnded)
            {
                _molesSpawnerService.RestartSpawnTimer();
            }

            var whacAMoleConfig = _gamePlayConfigService.GamePlayConfig.WhacAMole;
            var areCurrentlyMaxMoles = simulationState.Moles.Count >= whacAMoleConfig.MaxConcurrentMoles;

            if (isSpawnTimerEnded && !areCurrentlyMaxMoles)
            {
                SpawnMole(whacAMoleConfig);
            }
        }

        // The mole is only added to the state here, it stays out of the physics simulation until its hole finished shaking,
        // so nothing can target or hit it while it is still hidden.
        private void SpawnMole(WhacAMoleConfig whacAMoleConfig)
        {
            if (!TryFindAvailableSpawnPointPosition(whacAMoleConfig.MoleRadius, out var position))
            {
                return;
            }

            var isGolden = _molesSpawnerService.ShouldSpawnGoldenMole();
            var lives = (byte)(isGolden ? whacAMoleConfig.GoldenMoleLives : 1);
            var emergeOnTick = _processedTick + CalculateHoleShakeTicks();
            var mole = _matchDataService.AddMole(position, emergeOnTick, CalculateDisappearOnTick(whacAMoleConfig, emergeOnTick), isGolden, lives);
            _molesSpawnerService.RegisterMoleSpawned(isGolden);
            _netEventsDataService.AddMoleSpawnedNetEvent(_processedTick, mole.Id, position, emergeOnTick, isGolden, lives);
        }

        private void EmergeMolesWhoseHoleFinishedShaking()
        {
            var moles = _matchDataService.SimulationState.Moles;
            var moleRadius = _gamePlayConfigService.GamePlayConfig.WhacAMole.MoleRadius;

            for (var i = 0; i < moles.Count; i++)
            {
                ref var mole = ref moles.GetByIndex(i);

                if (mole.IsEmerged || _processedTick < mole.EmergeOnTick)
                {
                    continue;
                }

                mole.IsEmerged = true;
                _physicsSimulator.AddMole(mole.Id, mole.Position, moleRadius);
            }
        }

        private int CalculateHoleShakeTicks()
        {
            return (int)System.MathF.Ceiling(_sharedGamePlayConfig.MoleHoleShakeDurationSeconds * _networkConfig.TicksPerSeconds);
        }

        private int CalculateHideShakeTicks()
        {
            return (int)System.MathF.Ceiling(_sharedGamePlayConfig.MoleHideShakeDurationSeconds * _networkConfig.TicksPerSeconds);
        }

        // The lifetime only starts once the mole is actually out of its hole, the shake is not part of it.
        private int CalculateDisappearOnTick(WhacAMoleConfig whacAMoleConfig, int emergeOnTick)
        {
            if (whacAMoleConfig.MaxMoleLifetimeSeconds <= 0)
            {
                return NEVER_EXPIRES_TICK;
            }

            var lifetimeSeconds = RNG.NextFloat(whacAMoleConfig.MinMoleLifetimeSeconds, whacAMoleConfig.MaxMoleLifetimeSeconds);
            var lifetimeTicks = (int)System.MathF.Ceiling(lifetimeSeconds * _networkConfig.TicksPerSeconds);
            return emergeOnTick + lifetimeTicks;
        }

        // A mole whose lifetime ended does not vanish at once: it first shakes in place while staying hittable, and only
        // goes back into its hole once that shake is over. The expired net event is sent when the shake starts and carries
        // the hide tick, so no second event is needed once the mole is finally removed.
        private void ProcessExpiringMoles()
        {
            var moles = _matchDataService.SimulationState.Moles;

            for (int i = moles.Count - 1; i >= 0; i--)
            {
                ref var mole = ref moles.GetByIndex(i);

                if (mole.HideOnTick != NOT_EXPIRING_TICK)
                {
                    if (_processedTick < mole.HideOnTick)
                    {
                        continue;
                    }

                    if (mole.IsEmerged)
                    {
                        _physicsSimulator.RemoveMole(mole.Id);
                    }

                    moles.RemoveAt(i);
                    continue;
                }

                var hasReachedLifetimeEnd = mole.DisappearOnTick != NEVER_EXPIRES_TICK && _processedTick >= mole.DisappearOnTick;

                if (!hasReachedLifetimeEnd)
                {
                    continue;
                }

                mole.HideOnTick = _processedTick + CalculateHideShakeTicks();
                _netEventsDataService.AddMoleExpiredNetEvent(_processedTick, mole.Id, mole.HideOnTick);
            }
        }

        private bool TryFindAvailableSpawnPointPosition(float moleRadius, out Vector2 position)
        {
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
                var candidatePosition = spawnPoints[(startIndex + i) % spawnPoints.Length].Position * mapSizeMultiplier;
                var isSpawnPointFree = !_physicsSimulator.IsSquareHitAnyBodyTypes(candidatePosition, moleRadius, BLOCKING_SPAWN_BODY_TYPES)
                                       && !IsSpawnPointTakenByShakingMole(candidatePosition);

                if (isSpawnPointFree)
                {
                    position = candidatePosition;
                    return true;
                }
            }

            return false;
        }

        // A mole whose hole is still shaking has no physics body yet, so the physics check above cannot see it.
        private bool IsSpawnPointTakenByShakingMole(Vector2 candidatePosition)
        {
            foreach (var mole in _matchDataService.SimulationState.Moles.AsSpan())
            {
                if (!mole.IsEmerged && mole.Position == candidatePosition)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
