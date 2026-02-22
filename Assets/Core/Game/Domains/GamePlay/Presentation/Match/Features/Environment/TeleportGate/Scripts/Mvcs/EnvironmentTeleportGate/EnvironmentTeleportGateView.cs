using System.Threading;
using Core.Scripts.Extensions;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate
{
    public class EnvironmentTeleportGateView : MonoBehaviour
    {
        [SerializeField] private Transform _visuals;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private float _animationScale = 1.2f;
        [SerializeField] private float _animationDuration = 0.2f;
        private Vector2 _idleSize;
        CancellationTokenSource _bounceAnimationCancellationTokenSource;

        public void Setup(Sprite sprite, Vector2 size)
        {
            _renderer.sprite = sprite;
            _visuals.localScale = new Vector3(size.x, size.y, 1f);
            _idleSize = size;
        }

        public async Awaitable PlayBounceAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _bounceAnimationCancellationTokenSource?.Cancel();
            _bounceAnimationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            _visuals.localScale = new Vector3(_idleSize.x, _idleSize.y, 1f);
            // var halfAnimationDuration = _animationDuration * 0.5f;
            await _visuals.DOScale(_idleSize*_animationScale, _animationDuration)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .WithCancellationSafe(cancellationTokenSource.Token);
            // await _visuals.DOScale(_idleSize * _animationScale, halfAnimationDuration).WithCancellationSafe(cancellationTokenSource.Token);
            // await _visuals.DOScale(_idleSize, halfAnimationDuration).WithCancellationSafe(cancellationTokenSource.Token);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
