using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.States;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.State
{
    public class SimulationMatchState : IState // called from reflection
    {
        private readonly DiContainer _diContainer;
        private readonly ServerMatchInstaller _installer;
        private readonly ICommandFactory _commandFactory;
        private readonly SimulationMatchEnterData _enterData;

        public SimulationMatchState(DiContainer diContainer, SimulationMatchEnterData simulationMatchEnterData)
        {
            var newDiContainer = new DiContainer(diContainer);
            _enterData = simulationMatchEnterData;
            _installer = new ServerMatchInstaller(newDiContainer);
            _commandFactory = newDiContainer.Resolve<ICommandFactory>();
        }
        
        public void Enter()
        {
            _installer.InstallBindings();
            _commandFactory.CreateCommandVoid<ServerMatchEntryPointCommand>()
                .SetMatchEnterData(_enterData)
                .Execute();
        }

        public void Exit()
        {
            _commandFactory.CreateCommandVoid<ServerMatchExitPointCommand>().Execute();
            _installer.UninstallBindings();
        }
    }
}
