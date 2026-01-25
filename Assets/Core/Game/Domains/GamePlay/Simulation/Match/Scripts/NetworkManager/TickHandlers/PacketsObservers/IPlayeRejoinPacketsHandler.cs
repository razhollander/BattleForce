using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IPlayeRejoinPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ProcessPlayersRejoined(int processedTick);
    }
}