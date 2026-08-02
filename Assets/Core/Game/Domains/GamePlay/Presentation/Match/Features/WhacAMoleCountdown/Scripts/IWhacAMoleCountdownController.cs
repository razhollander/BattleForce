namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WhacAMoleCountdown.Scripts
{
    public interface IWhacAMoleCountdownController
    {
        void SetSecondsLeft(int secondsLeft);
        void HideCountdown();
    }
}
