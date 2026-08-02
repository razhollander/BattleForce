using System;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts
{
    public class HeadbuttHitEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _showDuration = 0.25f;
        [SerializeField] private Vector2 _startScale = new Vector2(0.4f, 0.4f);
        [SerializeField] private Vector2 _endScale = new Vector2(1.2f, 1.2f);
        private CancellationTokenSource _animationCancellationTokenSource;

        public Action Despawn { get; set; }

        public async Awaitable PlayAndDespawn(CancellationTokenSource cancellationTokenSource)
        {
            _animationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            transform.localScale = _startScale;

            var sequence = DOTween.Sequence();
            sequence.Join(transform.DOScale(_endScale, _showDuration).SetEase(Ease.OutQuad));
            if (_spriteRenderer != null)
            {
                var color = _spriteRenderer.color;
                color.a = 1f;
                _spriteRenderer.color = color;
                sequence.Join(_spriteRenderer.DOFade(0f, _showDuration).SetEase(Ease.InQuad));
            }
            
            try
            {
                await sequence.WithCancellationSafe(_animationCancellationTokenSource.Token);
            }
            finally
            {
                _animationCancellationTokenSource.Dispose();
                Despawn.Invoke();
            }
        }

        public void OnCreated()
        {
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
