using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts
{
    public class PreparationPhaseCountdownView : MonoBehaviour
    {
        [SerializeField] private Animation _animation;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private string _countdownAnimationClipName = "PreparationPhaseCountdown";

        public void SetCountdownTime(float elapsedTimeInSeconds)
        {
            SetIsShown(true);
            AnimationState state = _animation[_countdownAnimationClipName];
            _animation.Play(_countdownAnimationClipName);
            state.time = elapsedTimeInSeconds;
            _animation.Sample();
        }

        public void Stop()
        {
            _animation.Stop();
            SetIsShown(false);
        }

        private void SetIsShown(bool isShown)
        {
            _canvas.enabled = isShown;
        }
    }
}