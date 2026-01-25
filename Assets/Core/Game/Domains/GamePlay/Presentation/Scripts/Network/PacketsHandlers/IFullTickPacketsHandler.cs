namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public interface IFullTickPacketsHandler : IPacketsObserver
    {
        int LastProcessedTickFromServer { get; }
        void InitEntryPoint();
        int ProcessStateLatestTick(int clientTick);
        void InitExitPoint();
    }
}