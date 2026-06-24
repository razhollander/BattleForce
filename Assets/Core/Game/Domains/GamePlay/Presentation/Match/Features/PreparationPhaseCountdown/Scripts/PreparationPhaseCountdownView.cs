using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts
{
    public class PreparationPhaseCountdownView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _countdownAnimationStateName = "Countdown";

        public void Play(float elapsedTimeInSeconds)
        {
            gameObject.SetActive(true);
            _animator.Play(_countdownAnimationStateName, 0, 0f);
            _animator.Update(elapsedTimeInSeconds);
        }

        public void Stop()
        {
            gameObject.SetActive(false);
        }
    }
}
