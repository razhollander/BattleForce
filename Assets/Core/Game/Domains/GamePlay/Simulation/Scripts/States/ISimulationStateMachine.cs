using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.States
{
    public interface ISimulationStateMachine
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ChangeToMatchMaking();
        void ChangeToMatch(SimulationMatchEnterData enterData);
    }
}