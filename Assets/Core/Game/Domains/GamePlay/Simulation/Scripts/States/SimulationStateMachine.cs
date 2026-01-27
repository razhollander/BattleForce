using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.States;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Utils;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.States
{
    public interface ISimulationStateMachine
    {
        void ChangeTotMatchMaking();
        void ChangeToMatch(SimulationMatchEnterData enterData);
    }

    public class SimulationStateMachine : ISimulationStateMachine
    {
        private readonly DiContainer _diContainer;
        private IState _currentState;

        public SimulationStateMachine(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public void ChangeTotMatchMaking()
        {
            _currentState?.Exit();
            var matchMakingState = ReflectionUtils.CreateInstace("Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.State.SimulationMatchMakingState", "SimulationMatchMakingAssembly", _diContainer);
            _currentState = (IState) matchMakingState;
            _currentState.Enter(null);
        }
        
        public void ChangeToMatch(SimulationMatchEnterData enterData)
        {
            _currentState?.Exit();
            var matchState = ReflectionUtils.CreateInstace("Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator.ServerMatchInstaller", "SimulationMatchAssembly", _diContainer, enterData);
        }
    }
}
