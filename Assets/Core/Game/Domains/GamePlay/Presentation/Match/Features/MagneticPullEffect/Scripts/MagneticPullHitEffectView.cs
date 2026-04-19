using System;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts
{
    public class MagneticPullHitEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _showDuration = 1f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public async Awaitable PlayAndDespawn(Vector2 startPosition, Vector2 endPosition, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            var direction = (endPosition - startPosition).normalized;
            var distance = Vector2.Distance(startPosition, endPosition);
            var centerPosition = startPosition + direction * (distance / 2f);

            transform.position = centerPosition;
            transform.up = direction;
            transform.localScale = new Vector3(1f, distance, 1f);
            transform.SetParent(parent);

            if (_spriteRenderer != null)
            {
                var color = _spriteRenderer.color;
                color.a = 0;
                _spriteRenderer.color = color;

                _spriteRenderer.DOFade(1, 0.2f).WithCancellationSafe(cancellationTokenSource.Token).Forget();

                try
                {
                    await Awaitable.WaitForSecondsAsync(_showDuration - 0.2f, cancellationTokenSource.Token);
                    await _spriteRenderer.DOFade(0, 0.2f).WithCancellationSafe(cancellationTokenSource.Token);
                }
                finally
                {
                    Despawn();
                }
            }
            else
            {
                try
                {
                    await Awaitable.WaitForSecondsAsync(_showDuration, cancellationTokenSource.Token);
                }
                finally
                {
                    Despawn();
                }
            }
        }

        public void OnCreated()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public Action Despawn { get; set; }
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