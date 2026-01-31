using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.States;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerEntryPointCommand : BaseCommand, ICommandVoid
    {
        private IServerNetworkManager _serverNetworkManager;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationStateMachine _simulationStateMachine;
        private ITickService _tickService;
        private ISimulationPersistentData _simulationPersistentData;
        private IHeadLessQuitterController _headLessQuitterController;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationStateMachine = _diContainer.Resolve<ISimulationStateMachine>();
            _tickService = _diContainer.Resolve<ITickService>();
            _simulationPersistentData = _diContainer.Resolve<ISimulationPersistentData>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
        }

        public void Execute()
        {
            _serverNetworkManager.InitEntryPoint();
            _physicsSimulator.InitEntryPoint();
            _simulationStateMachine.InitEntryPoint();
            _simulationStateMachine.ChangeToMatchMaking();
            _tickService.InitEntryPoint();
            _simulationPersistentData.InitEntryPoint();
            _headLessQuitterController.InitEntryPoint();
        }
    }
}