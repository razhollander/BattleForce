using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers
{
    public interface IPlayerJoinPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ProcessPlayersJoined(int processedTick);
    }
}