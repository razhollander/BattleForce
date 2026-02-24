using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

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
        private EndStagePreparationPhaseCommand _endStagePreparationPhaseCommand;
        private Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage.IStageDataService _stageDataService;

        private float _deltaTime;
        private int _tick;
        private ProcessCachedCollisionsCommand _processCachedCollisionsCommand;

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
            _enforceFieldBarriersCommand = _commandFactory.CreateCommandVoid<EnforceFieldBarriersCommand>();
            _endStagePreparationPhaseCommand = _commandFactory.CreateCommandVoid<EndStagePreparationPhaseCommand>();
            _stageDataService = _diContainer.Resolve<Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage.IStageDataService>();
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
                _playersEngineLogic.TurnOnEngineIfPlayerIdle(playerState.Spaceship);
                _playersEngineLogic.TryAddEngineForceToPlayer(playerState.Spaceship, stepDeltaTime);
            }

            _stepAllWheelsRotationCommand.SetTime(_tick, stepDeltaTime).Execute();

            if (_stageDataService.PreparationPhaseTimer > 0)
            {
                _enforceFieldBarriersCommand.SetTick(_tick).Execute();
                _stageDataService.PreparationPhaseTimer -= stepDeltaTime;

                if (_stageDataService.PreparationPhaseTimer <= 0)
                {
                    _endStagePreparationPhaseCommand.SetTick(_tick).Execute();
                }
            }

            ApplyMatchModelToPhysicsSimulation();
            _physicsSimulator.Step(stepDeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
            ApplyPhysicsSimulationToMatchModel();
            
            _processCachedCollisionsCommand.SetProcessedTick(_tick).Execute();
        }

        private void ApplyMatchModelToPhysicsSimulation()
        {
            _physicsSimulator.CopyDataToSimulation(_matchDataService.SimulationState, _matchDataService.EnvironmentData.Walls, _matchDataService.EnvironmentData.LavaWalls, _matchDataService.EnvironmentData.Springs);
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
                ref var bulletState = ref _matchDataService.SimulationState.Bullets.GetByIndex(i);
                bulletState.Position = _physicsSimulator.GetBullet(bulletState.Id).Position;
            }

            for (int i = 0; i < _matchDataService.SimulationState.PowerUpBalls.Count; i++)
            {
                ref var powerUpBallState = ref _matchDataService.SimulationState.PowerUpBalls.GetByIndex(i);
                powerUpBallState.Position = _physicsSimulator.GetPowerUpBall(powerUpBallState.Id).Position;
            }
        }
    }
}