using System.Threading;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts
{
    public class ActivatePowerUpEffectView : MonoBehaviour
    {
        private const string ACTIVATE_POWERUP_ANIMATION_NAME = "ActivatePowerUpEffect";
        
        [SerializeField] private Animation _animation;
        private CancellationTokenSource _playingCancellationTokenSource;

        public async Awaitable PlayAnimation(CancellationToken cancellationToken)
        {
            _playingCancellationTokenSource?.Cancel();
            _playingCancellationTokenSource?.Dispose();
            _playingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            gameObject.SetActive(true);

            try
            {
                await _animation.PlayAsync(ACTIVATE_POWERUP_ANIMATION_NAME, _playingCancellationTokenSource.Token);
            }
            finally
            {
                gameObject.SetActive(false);
            }
        }
    }
}
