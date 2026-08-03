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
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;

            if (simulationState.StageType != StageType.WhacAMole)
            {
                return;
            }

            DespawnExpiredMoles();

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

        private void SpawnMole(WhacAMoleConfig whacAMoleConfig)
        {
            if (!TryFindAvailableSpawnPointPosition(whacAMoleConfig.MoleRadius, out var position))
            {
                return;
            }

            var mole = _matchDataService.AddMole(position, CalculateDisappearOnTick(whacAMoleConfig));
            _physicsSimulator.AddMole(mole.Id, position, whacAMoleConfig.MoleRadius);
            _netEventsDataService.AddMoleSpawnedNetEvent(_processedTick, mole.Id, position);
        }

        private int CalculateDisappearOnTick(WhacAMoleConfig whacAMoleConfig)
        {
            if (whacAMoleConfig.MaxMoleLifetimeSeconds <= 0)
            {
                return NEVER_EXPIRES_TICK;
            }

            var lifetimeSeconds = RNG.NextFloat(whacAMoleConfig.MinMoleLifetimeSeconds, whacAMoleConfig.MaxMoleLifetimeSeconds);
            var lifetimeTicks = (int)System.MathF.Ceiling(lifetimeSeconds * _networkConfig.TicksPerSeconds);
            return _processedTick + lifetimeTicks;
        }

        private void DespawnExpiredMoles()
        {
            var moles = _matchDataService.SimulationState.Moles;

            for (int i = moles.Count - 1; i >= 0; i--)
            {
                var mole = moles[i];
                var hasExpired = mole.DisappearOnTick != NEVER_EXPIRES_TICK && _processedTick >= mole.DisappearOnTick;

                if (!hasExpired)
                {
                    continue;
                }

                moles.RemoveAt(i);
                _physicsSimulator.RemoveMole(mole.Id);
                _netEventsDataService.AddMoleExpiredNetEvent(_processedTick, mole.Id);
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

                if (!_physicsSimulator.IsSquareHitAnyBodyTypes(candidatePosition, moleRadius, BLOCKING_SPAWN_BODY_TYPES))
                {
                    position = candidatePosition;
                    return true;
                }
            }

            return false;
        }
    }
}
