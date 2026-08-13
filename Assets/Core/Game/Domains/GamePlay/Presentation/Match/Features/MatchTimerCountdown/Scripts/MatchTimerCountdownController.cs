using System;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Consts;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MatchTimerCountdown.Scripts
{
    public class MatchTimerCountdownController : IMatchTimerCountdownController
    {
        private const int NO_COUNTDOWN_SHOWN = -1;
        private const int LAST_SECONDS_TICK_THRESHOLD = 5;

        private readonly MatchTimerCountdownView _view;
        private readonly IAudioService _audioService;
        private int _currentlyShownSecondsLeft = NO_COUNTDOWN_SHOWN;
        private bool _isCountdownShown = true; // the view is authored visible, so the very first hide still has to reach it

        public MatchTimerCountdownController(MatchTimerCountdownView view, IAudioService audioService)
        {
            _view = view;
            _audioService = audioService;
        }

        // The command below runs every frame, so the text is only rebuilt when the whole second actually changes.
        // Showing the countdown is Show()'s job alone - claiming it here would leave the flag on while the view stays
        // hidden, and every later Show() would then early-out on it.
        public void SetSecondsLeft(int secondsLeft)
        {
            if (_currentlyShownSecondsLeft == secondsLeft)
            {
                return;
            }

            _currentlyShownSecondsLeft = secondsLeft;
            _view.SetSecondsLeftText(TimeSpan.FromSeconds(secondsLeft).ToString(TimersConsts.MINUTES_SECONDS_FORMAT));

            if (secondsLeft > 0 && secondsLeft <= LAST_SECONDS_TICK_THRESHOLD)
            {
                _audioService.PlayAudio(AudioClipType.TimerTick);
                _view.PlayLastSecondTickAnimation();
            }
        }

        public void Show()
        {
            if (_isCountdownShown)
            {
                return;
            }

            _isCountdownShown = true;
            _view.Show();
        }

        public void Hide()
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
