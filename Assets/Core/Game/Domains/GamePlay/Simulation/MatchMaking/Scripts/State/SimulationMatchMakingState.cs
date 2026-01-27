using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.States;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.State
{
    public class SimulationMatchMakingState : IState // called from reflection
    {
        private readonly DiContainer _diContainer;
        private readonly ServerMatchMakingInstaller _installer;
        private readonly ICommandFactory _commandFactory;

        public SimulationMatchMakingState(DiContainer diContainer)
        {
            var newDiContainer = new DiContainer(diContainer);
            _installer = new ServerMatchMakingInstaller(newDiContainer);
            _commandFactory = newDiContainer.Resolve<ICommandFactory>();
        }

        public void Enter(object enterData)
        {
            _installer.InstallBindings();
            _commandFactory.CreateCommandVoid<ServerMatchMakingEntryPointCommand>().Execute();
        }

        public void Exit()
        {
            _commandFactory.CreateCommandVoid<ServerMatchMakingExitPointCommand>().Execute();
            _installer.UninstallBindings();
        }
    }
}
