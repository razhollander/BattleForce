namespace Core.Game.Domains.GamePlay.Presentation.Scripts.DataService
{
    public interface INetworkDiagnosticsService
    {
        string LastReportText { get; }
        void OnFullTickPacketReceived();
        void OnStateProcessed(int ticksAdvancedSinceLastProcessedState);
        void OnPollCompleted(int pingInMilliseconds);
        void OnFrameRendered(float frameDeltaTimeInSeconds);
    }
}
