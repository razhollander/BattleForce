namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts
{
    public class PreparationPhaseCountdownController : IPreparationPhaseCountdownController
    {
        private readonly PreparationPhaseCountdownView _view;

        public bool IsCountdownPlaying { private set; get; }
        public PreparationPhaseCountdownController(PreparationPhaseCountdownView view)
        {
            _view = view;
        }


        public void SetCountdownTime(float elapsedSeconds)
        {
            IsCountdownPlaying = true;
            _view.SetCountdownTime(elapsedSeconds);
        }

        public void StopCountdown()
        {
            IsCountdownPlaying = false;
            _view.Stop();
        }
    }
}
