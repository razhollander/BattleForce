using System;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MoveDestinationPointIndicator.Scripts
{
    public class MoveDestinationPointIndicatorView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _fadeOutDurationInSeconds = 0.5f;

        public Action Despawn { get; set; }

        private CancellationTokenSource _fadeCancellationTokenSource;

        public async Awaitable PlayAndDespawn(CancellationTokenSource cancellationTokenSource)
        {
            _fadeCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            ResetToFullyOpaque();

            try
            {
                await _spriteRenderer.DOFade(0f, _fadeOutDurationInSeconds).SetEase(Ease.InQuad).WithCancellationSafe(_fadeCancellationTokenSource.Token);
            }
            finally
            {
                _fadeCancellationTokenSource.Dispose();
                Despawn.Invoke();
            }
        }

        private void ResetToFullyOpaque()
        {
            var color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
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
