namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers
{
    public interface IPlayerJoinPacketsHandler
    {
        void RegisterListeners();
        void UnregisterListeners();
        void InitExitPoint();
    }
}