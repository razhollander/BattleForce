namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers
{
    public interface IPlayerJoinPacketsHandler
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ProcessPlayersJoined(int processedTick);
    }
}