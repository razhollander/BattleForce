using System;
using CoreDomain.Scripts.Consts;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WhacAMoleCountdown.Scripts
{
    public class WhacAMoleCountdownController : IWhacAMoleCountdownController
    {
        private const int NO_COUNTDOWN_SHOWN = -1;

        private readonly WhacAMoleCountdownView _view;
        private int _currentlyShownSecondsLeft = NO_COUNTDOWN_SHOWN;
        private bool _isCountdownShown = true; // the view is authored visible, so the very first hide still has to reach it

        public WhacAMoleCountdownController(WhacAMoleCountdownView view)
        {
            _view = view;
        }

        // The command below runs every frame, so the text is only rebuilt when the whole second actually changes.
        public void SetSecondsLeft(int secondsLeft)
        {
            if (_isCountdownShown && _currentlyShownSecondsLeft == secondsLeft)
            {
                return;
            }

            _isCountdownShown = true;
            _currentlyShownSecondsLeft = secondsLeft;
            _view.SetSecondsLeftText(TimeSpan.FromSeconds(secondsLeft).ToString(TimersConsts.MINUTES_SECONDS_FORMAT));
        }

        public void HideCountdown()
        {
            if (!_isCountdownShown)
            {
                return;
            }

            _isCountdownShown = false;
            _currentlyShownSecondsLeft = NO_COUNTDOWN_SHOWN;
            _view.Hide();
        }
    }
}
