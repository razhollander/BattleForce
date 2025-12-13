namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public interface IFullTickPacketsHandler
    {
        int LastProcessedTickFromServer { get; }
        void RegisterListeners();
        void ProcessStateLatestTick();
        void InitExitPoint();
    }
}