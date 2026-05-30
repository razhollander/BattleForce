using System;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetEffectView : MonoBehaviour, IPoolable
    {
        private const string LOCK_ON_TARGET_ANIMATION_NAME = "LockOnTarget";
        
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Animation _animation;
        [SerializeField] private float _hitLineWidth = 0.2f;
        [SerializeField] private float _idleLineWidth = 0.1f;
        [SerializeField] private float _lineHitDurationInSeconds = 0.3f;
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
            _lineRenderer.startWidth = _idleLineWidth;
            _lineRenderer.endWidth = _idleLineWidth;
            _currentAnimationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            while (!_currentAnimationCancellationTokenSource.IsCancellationRequested)
            {
                await _animation.PlayAsync(LOCK_ON_TARGET_ANIMATION_NAME, cancellationToken: _currentAnimationCancellationTokenSource.Token);
                PlayLineHitAnimation(_currentAnimationCancellationTokenSource.Token).Forget();
            }
        }

        private async Awaitable PlayLineHitAnimation(CancellationToken cancellationToken)
        {
            _lineRenderer.startWidth = _hitLineWidth;
            _lineRenderer.endWidth = _hitLineWidth;
            await Awaitable.WaitForSecondsAsync(_lineHitDurationInSeconds, cancellationToken);
            _lineRenderer.startWidth = _idleLineWidth;
            _lineRenderer.endWidth = _idleLineWidth;
        }

        public void OnDespawned()
        {
            _currentAnimationCancellationTokenSource.Cancel();
            gameObject.SetActive(false);
        }

        public void Setup(float lockOnTargetDurationInSeconds)
        {
            _animation[LOCK_ON_TARGET_ANIMATION_NAME].speed = 1f/lockOnTargetDurationInSeconds;
        }
        
        public void UpdatePosition(Vector2 lineStartPoint, Vector2 lineEndPoint, Vector2 targetPosition)
        {
            transform.position = targetPosition;
            _lineRenderer.SetPosition(0, lineStartPoint);
            _lineRenderer.SetPosition(1, lineEndPoint);
        }
    }
}
