namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public interface INetworkTimerService
    {
        string StartTimer(int endServerTick);
        void CancelTimer(string timerGuid);
        float GetTimerSecondsLeft(string timerGuid, int currentServerTick);
    }
}
