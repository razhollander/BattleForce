using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Services.ApplicationSubscriptionService;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.States
{
    public class SimulationStateMachine : ISimulationStateMachine, IApplicationObserver
    {
        private readonly DiContainer _diContainer;
        private readonly IApplicationSubscriptionService _applicationSubscriptionService;
        private IState _currentState;

        public SimulationStateMachine(DiContainer diContainer, IApplicationSubscriptionService applicationSubscriptionService)
        {
            _diContainer = diContainer;
            _applicationSubscriptionService = applicationSubscriptionService;
        }

        public void InitEntryPoint()
        {
            _applicationSubscriptionService.RegisterObserver(this);
        }

        public void InitExitPoint()
        {
            ExitCurerntState();
            _applicationSubscriptionService.UnregisterObserver(this);
        }
        
        public void ChangeToMatchMaking()
        {
            ExitCurerntState();
            var matchMakingState = ReflectionUtils.CreateInstace("Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.State.SimulationMatchMakingState", "SimulationMatchMakingAssembly", _diContainer);
            _currentState = (IState) matchMakingState;
            _currentState.Enter();
        }
        
        public void ChangeToMatch(SimulationMatchEnterData enterData)
        {
            ExitCurerntState();
            var matchState = ReflectionUtils.CreateInstace("Core.Game.Domains.GamePlay.Simulation.Match.Scripts.State.SimulationMatchState", "SimulationMatchAssembly", _diContainer, enterData);
            _currentState = (IState) matchState;
            _currentState.Enter();
        }

        public void OnApplicationQuit()
        {
            ExitCurerntState();
        }

        private void ExitCurerntState()
        {
            _currentState?.Exit(); // todo: move closing the thread to the simulation gameplay domain
        }
        
        public void OnApplicationFocus(bool hasFocus)
        {
        }
    }
}
