namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public interface ISimulationStatePacketsHandler
    {
        int LatestTickProcessedFromServer { get; }
        void RegisterListeners();
        void ProcessStateLatestTick();
        void InitExitPoint();
    }
}