using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts
{
    public class EnvironmentSpikeView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _strechedSprite;
        [SerializeField] private Sprite _pressedSprite;
        [SerializeField] private float _animationDuration;

        CancellationTokenSource _hitAnimationCancellationTokenSource;

        public async Awaitable PlayHitAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _hitAnimationCancellationTokenSource?.Cancel();
            _hitAnimationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            _renderer.sprite = _strechedSprite;

            try
            {
                await Awaitable.WaitForSecondsAsync(_animationDuration, _hitAnimationCancellationTokenSource.Token);
            }
            finally
            {
                _renderer.sprite = _pressedSprite;
            }
        }
    }
}
