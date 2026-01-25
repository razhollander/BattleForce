namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers.PacketsHandlers
{
    public interface IPlayerJoinPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ProcessPlayersJoined(int processedTick);
    }
}