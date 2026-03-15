namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public interface INetworkTimerService
    {
        string StartTimer(int initialTick);
        void CancelTimer(string timerGuid);
        float GetTimerSecondsLeft(string timerGuid);
    }
}
