using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts
{
    public class EnvironmentSpringView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _strechedSprite;
        [SerializeField] private Sprite _pressedSprite;
        [SerializeField] private float _animationDuration;
        
        CancellationTokenSource _bounceAnimationCancellationTokenSource;
        
        public async Awaitable PlayBounceAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _bounceAnimationCancellationTokenSource?.Cancel();
            _bounceAnimationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            _renderer.sprite = _strechedSprite;
            await Awaitable.WaitForSecondsAsync(_animationDuration, _bounceAnimationCancellationTokenSource.Token);
            _renderer.sprite = _pressedSprite;
        }
    }
}
