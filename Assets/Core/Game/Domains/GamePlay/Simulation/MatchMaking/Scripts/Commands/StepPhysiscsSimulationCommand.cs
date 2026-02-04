using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands
{
    public class StepPhysiscsSimulationCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matcMakinghDataService;
        private IPhysicsSimulator _physicsSimulator;
        private NetworkConfig _networkConfig;
        private ICommandFactory _commandFactory;
        private MatchMakingProcessCachedCollisionsCommand _processCachedCollisionsCommand;

        private float _deltaTime;
        private int _tick;

        public StepPhysiscsSimulationCommand SetDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }
        
        public StepPhysiscsSimulationCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matcMakinghDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _processCachedCollisionsCommand = _commandFactory.CreateCommandVoid<MatchMakingProcessCachedCollisionsCommand>();
        }

        public void Execute()
        {
            StepPhysics(_deltaTime);
        }

        private void StepPhysics(float stepDeltaTime)
        {
            ApplyMatchModelToPhysicsSimulation();
            _physicsSimulator.Step(stepDeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
            ApplyPhysicsSimulationToMatchModel();
            _processCachedCollisionsCommand.SetProcessedTick(_tick).Execute();
        }

        private void ApplyMatchModelToPhysicsSimulation()
        {
            _physicsSimulator.CopyDataToSimulation(_matcMakinghDataService.SimulationState);
        }

        private void ApplyPhysicsSimulationToMatchModel()
        {
            for (int i = 0; i < _matcMakinghDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matcMakinghDataService.SimulationState.Players.GetByIndex(i);
                playerState.Spaceship.Transform.Position = _physicsSimulator.GetPlayer(playerState.Id).Position;
            }

            for (int i = 0; i < _matcMakinghDataService.SimulationState.Bullets.Count; i++)
            {
                ref var bulletState = ref _matcMakinghDataService.SimulationState.Bullets.GetByIndex(i);
                bulletState.Position = _physicsSimulator.GetBullet(bulletState.Id).Position;
            }
        }
    }
}