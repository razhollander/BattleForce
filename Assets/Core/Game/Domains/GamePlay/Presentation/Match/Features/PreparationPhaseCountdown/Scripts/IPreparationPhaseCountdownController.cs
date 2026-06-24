namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts
{
    public interface IPreparationPhaseCountdownController
    {
        void PlayCountdown(int elapsedTicks);
        void StopCountdown();
    }
}
