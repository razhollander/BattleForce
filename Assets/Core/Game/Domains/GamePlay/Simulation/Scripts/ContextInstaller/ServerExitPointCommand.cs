using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.States;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerExitPointCommand: BaseCommand, ICommandVoid
    {
        private IServerNetworkManager _serverNetworkManager;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationStateMachine _simulationStateMachine;
        private ITickService _tickService;
        private IHeadLessQuitterController _headLessQuitterController;
        private ISimulationSpeedupController _simulationSpeedupController;
        private ISimulationGamePlayConfigService _simulationGamePlayConfigService;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationStateMachine = _diContainer.Resolve<ISimulationStateMachine>();
            _tickService = _diContainer.Resolve<ITickService>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _simulationSpeedupController = _diContainer.Resolve<ISimulationSpeedupController>();
            _simulationGamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
        }

        public void Execute()
        {
            _simulationGamePlayConfigService.InitExitPoint();
            _serverNetworkManager.InitExitPoint();
            _physicsSimulator.InitExitPoint();
            _simulationStateMachine.InitExitPoint();
            _headLessQuitterController.InitExitPoint();
            _simulationSpeedupController.InitExitPoint();
            
            _tickService.StopTick(); // must be last this stops the thread
        }
    }
}