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
        public Transform Transform;
        CancellationTokenSource _bounceAnimationCancellationTokenSource;

        public void Setup(Sprite sprite, Vector2 size)
        {
            _renderer.sprite = sprite;
            _idleSize = size;
            Transform = transform;
            ResetToIdleScale();
        }

        private void ResetToIdleScale()
        {
            _visuals.localScale = new Vector3(_idleSize.x, _idleSize.y, 1f);
        }
        
        public async Awaitable PlayBounceAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _bounceAnimationCancellationTokenSource?.Cancel();
            _bounceAnimationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            ResetToIdleScale();
            await _visuals.DOScale(_idleSize*_animationScale, _animationDuration)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .WithCancellationSafe(cancellationTokenSource.Token);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
