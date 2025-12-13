namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public interface IFullTickPacketsHandler
    {
        int LastProcessedTickFromServer { get; }
        void RegisterListeners();
        int ProcessStateLatestTick(int clientTick);
        void InitExitPoint();
    }
}