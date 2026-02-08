using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IMatchPlayerJoinPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ProcessPlayersJoined(int processedTick);
    }
}