using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts
{
    public class PreparationPhaseCountdownView : MonoBehaviour
    {
        [SerializeField] private Animation _animation; 
        [SerializeField] private string _countdownAnimationClipName = "PreparationPhaseCountdown";

        public void Play(float elapsedTimeInSeconds)
        {
            gameObject.SetActive(true);
            AnimationState state = _animation[_countdownAnimationClipName];
            _animation.Play(_countdownAnimationClipName);
            state.time = elapsedTimeInSeconds;
            _animation.Sample();
        }

        public void Stop()
        {
            _animation.Stop(); 
            gameObject.SetActive(false);
        }
    }
}