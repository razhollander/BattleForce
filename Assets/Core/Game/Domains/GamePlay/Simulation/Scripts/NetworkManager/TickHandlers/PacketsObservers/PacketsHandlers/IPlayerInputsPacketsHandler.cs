using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsHandlers;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers.PacketsHandlers
{
    public interface IPlayerInputsPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        ProcessPlayersInputsResult ProcessInputs(int processedTick);
        void InitExitPoint();
    }
}