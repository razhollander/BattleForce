namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WhacAMoleCountdown.Scripts
{
    public class WhacAMoleCountdownController : IWhacAMoleCountdownController
    {
        private const int NO_COUNTDOWN_SHOWN = -1;

        private readonly WhacAMoleCountdownView _view;
        private int _currentlyShownSecondsLeft = NO_COUNTDOWN_SHOWN;

        public WhacAMoleCountdownController(WhacAMoleCountdownView view)
        {
            _view = view;
        }

        // The command below runs every frame, so the text is only rebuilt when the whole second actually changes.
        public void SetSecondsLeft(int secondsLeft)
        {
            if (_currentlyShownSecondsLeft == secondsLeft)
            {
                return;
            }

            _currentlyShownSecondsLeft = secondsLeft;
            _view.SetSecondsLeftText(secondsLeft.ToString());
        }

        public void HideCountdown()
        {
            if (_currentlyShownSecondsLeft == NO_COUNTDOWN_SHOWN)
            {
                return;
            }

            _currentlyShownSecondsLeft = NO_COUNTDOWN_SHOWN;
            _view.Hide();
        }
    }
}
