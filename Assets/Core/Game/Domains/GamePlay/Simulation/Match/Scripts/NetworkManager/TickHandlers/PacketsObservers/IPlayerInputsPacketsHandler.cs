using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IPlayerInputsPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        ProcessPlayersInputsResult ProcessInputs(int processedTick);
        void InitExitPoint();
    }
}