using System.Diagnostics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepPhysiscsSimulationCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private NetworkConfig _networkConfig;
        private IPlayersDecelerationLogic _playersDecelerationLogic;
        private IPlayersEngineLogic _playersEngineLogic;
        private ICommandFactory _commandFactory;
        private StepAllWheelsRotationCommand _stepAllWheelsRotationCommand;
        private EnforceFieldBarriersCommand _enforceFieldBarriersCommand;
        private EnforceStageBarriersCommand _enforceStageBarriersCommand;

        private float _deltaTime;
        private int _tick;
        private ProcessCachedCollisionsCommand _processCachedCollisionsCommand;
        private AddNormalForceToPlayerStickWithWallCommand _addNormalForceToPlayerStickWithWallCommand;

        public StepPhysiscsSimulationCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }
        
        public StepPhysiscsSimulationCommand SetDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _playersDecelerationLogic = _diContainer.Resolve<IPlayersDecelerationLogic>();
            _playersEngineLogic = _diContainer.Resolve<IPlayersEngineLogic>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _stepAllWheelsRotationCommand = _commandFactory.CreateCommandVoid<StepAllWheelsRotationCommand>();
            _processCachedCollisionsCommand = _commandFactory.CreateCommandVoid<ProcessCachedCollisionsCommand>();
            _addNormalForceToPlayerStickWithWallCommand = _commandFactory.CreateCommandVoid<AddNormalForceToPlayerStickWithWallCommand>();
            _enforceFieldBarriersCommand = _commandFactory.CreateCommandVoid<EnforceFieldBarriersCommand>();
            _enforceStageBarriersCommand = _commandFactory.CreateCommandVoid<EnforceStageBarriersCommand>();
        }

        public void Execute()
        {
            StepPhysics(_deltaTime);
        }

        private void StepPhysics(float stepDeltaTime)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                _playersDecelerationLogic.DeceleratePlayerVelocity(playerState.Spaceship, stepDeltaTime);
                _playersDecelerationLogic.DeceleratePlayerSpin(playerState.Spaceship, stepDeltaTime);
                _playersEngineLogic.TurnOnEngineForPlayerIfPossible(playerState.Spaceship);
                _playersEngineLogic.TryAddEngineForceToPlayer(playerState.Spaceship, stepDeltaTime);
            }

            if (!_matchDataService.SimulationState.IsInPreparationPhase)
            {
                _stepAllWheelsRotationCommand.SetTime(_tick, stepDeltaTime).Execute();
            }

            GuardAgainstNonFinitePlayerState();
            ApplyMatchModelToPhysicsSimulation();
            _physicsSimulator.Step(stepDeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
            ApplyPhysicsSimulationToMatchModel();
            
            _processCachedCollisionsCommand.SetProcessedTick(_tick).Execute();
            _addNormalForceToPlayerStickWithWallCommand.SetTick(_tick).Execute();
            _enforceFieldBarriersCommand.SetTick(_tick).Execute();
        }

        [Conditional("ERROR_LOGS_ENABLED")]
        private void GuardAgainstNonFinitePlayerState()
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                ref var transform = ref playerState.Spaceship.Transform;

                if (!transform.Velocity.IsFinite())
                {
                    LogService.LogError($"[NaNGuard] Player {playerState.Id} non-finite Velocity={transform.Velocity} (Position={transform.Position}, Direction={transform.Direction}, tick={_tick}). Resetting to zero.");
                    transform.Velocity = Vector2.Zero;
                }

                if (!transform.Direction.IsFinite())
                {
                    LogService.LogError($"[NaNGuard] Player {playerState.Id} non-finite Direction={transform.Direction} (Position={transform.Position}, Velocity={transform.Velocity}, tick={_tick}). Resetting to UnitX.");
                    transform.Direction = Vector2.UnitX;
                }

                if (!transform.Position.IsFinite())
                {
                    LogService.LogError($"[NaNGuard] Player {playerState.Id} non-finite Position={transform.Position} (Velocity={transform.Velocity}, Direction={transform.Direction}, tick={_tick}).");
                }
            }
        }

        private void ApplyMatchModelToPhysicsSimulation()
        {
            _physicsSimulator.CopyDataToSimulation(_matchDataService.SimulationState, _matchDataService.EnvironmentData.Walls, _matchDataService.EnvironmentData.LavaWalls, _matchDataService.EnvironmentData.Springs, _matchDataService.EnvironmentData.Spikes, _matchDataService.EnvironmentData.TeleportGates);
        }

        private void ApplyPhysicsSimulationToMatchModel()
        {
            for (int i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.Players.GetByIndex(i);
                playerState.Spaceship.Transform.Position = _physicsSimulator.GetPlayer(playerState.Id).Position;
            }

            for (int i = 0; i < _matchDataService.SimulationState.Bullets.Count; i++)
            {
                ref var bulletState = ref _matchDataService.SimulationState.Bullets.Get(i);
                bulletState.Position = _physicsSimulator.GetBullet(bulletState.Id).Position;
            }

            for (int i = 0; i < _matchDataService.SimulationState.PowerUpBalls.Count; i++)
            {
                ref var powerUpBallState = ref _matchDataService.SimulationState.PowerUpBalls.GetByIndex(i);
                powerUpBallState.Position = _physicsSimulator.GetPowerUpBall(powerUpBallState.Id).Position;
            }
            
            for (int i = 0; i < _matchDataService.SimulationState.KOProjectiles.Count; i++)
            {
                ref var koProjectileState = ref _matchDataService.SimulationState.KOProjectiles.GetByIndex(i);
                koProjectileState.Position = _physicsSimulator.GetKOProjectile(koProjectileState.Id).Position;
            }
            
            for (int i = 0; i < _matchDataService.SimulationState.GrapplingHookProjectiles.Count; i++)
            {
                ref var grapplingHookProjectileId = ref _matchDataService.SimulationState.GrapplingHookProjectiles.GetByIndex(i);
                grapplingHookProjectileId.Position = _physicsSimulator.GetGrapplingHookProjectile(grapplingHookProjectileId.Id).Position;
            }
        }
    }
}