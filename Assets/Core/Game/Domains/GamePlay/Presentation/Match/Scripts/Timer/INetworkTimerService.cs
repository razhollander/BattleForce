namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public interface INetworkTimerService
    {
        string StartTimer(int initialServerTick);
        void CancelTimer(string timerGuid);
        float GetTimerSecondsPassed(string timerGuid, int currentServerTick);
    }
}
