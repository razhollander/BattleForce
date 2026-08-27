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
        private const float RETENTION_BAR_EMPTY_HALF_ARC_DEGREES = 180f;
        private const float RETENTION_BAR_FULL_HALF_ARC_DEGREES = 0f;

        private static readonly int HALF_ARC_SHADER_PROPERTY = Shader.PropertyToID("_HalfArc");

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Animation _animation;
        [SerializeField] private float _hitLineWidth = 0.2f;
        [SerializeField] private float _idleLineWidth = 0.1f;
        [SerializeField] private Sprite _shootableSprite;
        [SerializeField] private Sprite _lockOnTargetSprite;
        [SerializeField] private Color _lineColorLockOnTarget = Color.white;
        [SerializeField] private Color _lineColorShootable = Color.white;
        [SerializeField] private Color _lineColorRetention = new Color(1f, 0.61960787f, 0.23921569f, 1f);
        [SerializeField] private GameObject _retentionRadialProgressBar;
        [SerializeField] private SpriteRenderer _retentionRadialProgressBarSpriteRenderer;

        private CancellationTokenSource _currentAnimationCancellationTokenSource;
        private Material _retentionRadialProgressBarMaterial;

        public Action Despawn { get; set; }

        public void OnCreated()
        {
            _retentionRadialProgressBarMaterial = _retentionRadialProgressBarSpriteRenderer.material;
        }

        public void OnSpawned()
        {
            HideRetentionEffect();
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
            HideRetentionEffect();
            gameObject.SetActive(false);
        }

        public void ShowRetentionEffect(float retentionProgress)
        {
            _retentionRadialProgressBar.TrySetActive(true);
            var halfArcDegrees = Mathf.Lerp(RETENTION_BAR_EMPTY_HALF_ARC_DEGREES, RETENTION_BAR_FULL_HALF_ARC_DEGREES, retentionProgress);
            _retentionRadialProgressBarMaterial.SetFloat(HALF_ARC_SHADER_PROPERTY, halfArcDegrees);

            var lineWidth = _hitLineWidth * retentionProgress;
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth;
            _lineRenderer.startColor = _lineColorRetention;
            _lineRenderer.endColor = _lineColorRetention;
        }

        public void HideRetentionEffect()
        {
            _retentionRadialProgressBar.TrySetActive(false);
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
