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
        
        public void SetSecondsLeft(int secondsLeft)
        {
            var isTheSameSecondsAsShown = _currentlyShownSecondsLeft == secondsLeft;
            if (isTheSameSecondsAsShown)
            {
                return;
            }

            _currentlyShownSecondsLeft = secondsLeft;
            _view.SetSecondsLeftText(TimeSpan.FromSeconds(secondsLeft).ToString(TimersConsts.MINUTES_SECONDS_FORMAT));

            var isInLastSecondsCountdown = secondsLeft is > 0 and <= LAST_SECONDS_TICK_THRESHOLD;
            if (isInLastSecondsCountdown)
            {
                PlayLastSecondCountdownAnimation();
            }
        }

        private void PlayLastSecondCountdownAnimation()
        {
            _audioService.PlayAudio(AudioClipType.TimerTick);
            _view.PlayLastSecondTickAnimation();
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
