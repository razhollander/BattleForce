namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MatchTimerCountdown.Scripts
{
    public interface IMatchTimerCountdownController
    {
        void SetSecondsLeft(int secondsLeft);
        void HideCountdown();
    }
}
