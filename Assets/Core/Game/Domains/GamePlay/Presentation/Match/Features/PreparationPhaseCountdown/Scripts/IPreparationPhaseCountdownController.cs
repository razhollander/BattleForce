namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts
{
    public interface IPreparationPhaseCountdownController
    {
        bool IsCountdownPlaying { get; }
        void SetCountdownTime(float elapsedSeconds);
        void StopCountdown();
    }
}
