namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public interface IFullTickPacketsHandler : IPacketsObserver
    {
        int LastProcessedTickFromServer { get; }
        void InitEntryPoint();
        void ProcessStateLatestTick();
        void ClearUnprocessedPacketsByView();
        void InitExitPoint();
    }
}