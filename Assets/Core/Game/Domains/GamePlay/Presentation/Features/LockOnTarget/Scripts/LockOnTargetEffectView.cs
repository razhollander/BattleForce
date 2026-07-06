using System;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public class LockOnTargetEffectView : MonoBehaviour, IPoolable
    {
        private const string LOCK_ON_TARGET_ANIMATION_NAME = "LockOnTarget";
        private const string LOCK_ON_TARGET_SHOOTABLE_ANIMATION_NAME = "LockOnTargetShootable";

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Animation _animation;
        [SerializeField] private float _hitLineWidth = 0.2f;
        [SerializeField] private float _idleLineWidth = 0.1f;
        [SerializeField] private Sprite _shootableSprite;
        [SerializeField] private Sprite _lockOnTargetSprite;
        [SerializeField] private Color _lineColorLockOnTarget = Color.white;
        [SerializeField] private Color _lineColorShootable = Color.white;

        private CancellationTokenSource _currentAnimationCancellationTokenSource;

        public Action Despawn { get; set; }

        public void OnCreated()
        {
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public async Awaitable PlayLockOnTargetAnimation(CancellationToken cancellationToken)
        {
            var animationCancellationTokenSource = RestartAnimationCancellationTokenSource(cancellationToken);
            _lineRenderer.startWidth = _idleLineWidth;
            _lineRenderer.endWidth = _idleLineWidth;
            _lineRenderer.startColor = _lineColorLockOnTarget;
            _lineRenderer.endColor = _lineColorLockOnTarget;
            _spriteRenderer.sprite = _lockOnTargetSprite;
            await _animation.PlayAsync(LOCK_ON_TARGET_ANIMATION_NAME, cancellationToken: animationCancellationTokenSource.Token);
        }

        public async Awaitable PlayLockOnTargetShootableAnimation(CancellationToken cancellationToken)
        {
            var animationCancellationTokenSource = RestartAnimationCancellationTokenSource(cancellationToken);
            _lineRenderer.startWidth = _hitLineWidth;
            _lineRenderer.endWidth = _hitLineWidth;
            _lineRenderer.startColor = _lineColorShootable;
            _lineRenderer.endColor = _lineColorShootable;
            _spriteRenderer.sprite = _shootableSprite;
            await _animation.PlayAsync(LOCK_ON_TARGET_SHOOTABLE_ANIMATION_NAME, cancellationToken: animationCancellationTokenSource.Token);
        }

        private CancellationTokenSource RestartAnimationCancellationTokenSource(CancellationToken cancellationToken)
        {
            _currentAnimationCancellationTokenSource?.Cancel();
            _currentAnimationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return _currentAnimationCancellationTokenSource;
        }

        public void OnDespawned()
        {
            _currentAnimationCancellationTokenSource?.Cancel();
            gameObject.SetActive(false);
        }

        public void Setup(float lockOnTargetDurationInSeconds)
        {
            _animation[LOCK_ON_TARGET_ANIMATION_NAME].speed = 1f / lockOnTargetDurationInSeconds;
        }
        
        public void UpdatePosition(Vector2 lineStartPoint, Vector2 lineEndPoint, Vector2 targetPosition)
        {
            transform.position = targetPosition;
            _lineRenderer.SetPosition(0, lineStartPoint);
            _lineRenderer.SetPosition(1, lineEndPoint);
        }
    }
}
