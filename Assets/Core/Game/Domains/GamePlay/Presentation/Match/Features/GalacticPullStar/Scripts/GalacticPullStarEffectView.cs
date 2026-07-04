using System;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts
{
    public class GalacticPullStarEffectView : MonoBehaviour, IPoolable
    {
        private static readonly int OUTLINE_SHADER_PROPERTY = Shader.PropertyToID("_OutlineColor");

        [SerializeField] private SpriteRenderer _starSpriteRenderer;
        [SerializeField] private SpriteRenderer _gravityForceSpriteRenderer;
        [SerializeField] private float _shownScale = 100f;
        [SerializeField] private float _scaleInDurationInSeconds = 0.35f;
        [SerializeField] private Ease _scaleInEase = Ease.OutBounce;
        [SerializeField] private float _slideOutOffsetY = 6f;
        [SerializeField] private float _slideOutDurationInSeconds = 0.25f;
        [SerializeField] private float _reflowDurationInSeconds = 0.2f;

        private Material _starMaterial;
        private CancellationTokenSource _localMoveCancellationTokenSource;

        public Action Despawn { get; set; }

        public void Setup(Color outlineColor, GalacticStarVisualData visualData)
        {
            _starMaterial.SetColor(OUTLINE_SHADER_PROPERTY, outlineColor);
            _starSpriteRenderer.sprite = visualData.PlanetSprite;
            _gravityForceSpriteRenderer.sharedMaterial = visualData.GravityForceMaterial;
        }
        
        public void SetSortingOrder(int order)
        {
            _starSpriteRenderer.sortingOrder = order;
            _gravityForceSpriteRenderer.sortingOrder = order;
        }

        public async Awaitable ScaleInAsync(float targetLocalY, CancellationToken cancellationToken)
        {
            transform.localPosition = new Vector3(0f, targetLocalY, 0f);
            transform.localScale = Vector3.zero;
            await transform.DOScale(_shownScale, _scaleInDurationInSeconds)
                .SetEase(_scaleInEase)
                .WithCancellationSafe(cancellationToken);
        }

        public async Awaitable MoveToSlotAsync(float targetLocalY, CancellationToken cancellationToken)
        {
            CancelLocalMoveCancellationTokenSource();
            _localMoveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await transform.DOLocalMoveY(targetLocalY, _reflowDurationInSeconds)
                .SetEase(Ease.OutQuad)
                .WithCancellationSafe(_localMoveCancellationTokenSource.Token);
        }

        public async Awaitable SlideOutAsync(CancellationToken cancellationToken)
        {
            try
            {
                CancelLocalMoveCancellationTokenSource();
                _localMoveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                await transform.DOLocalMoveY(transform.localPosition.y - _slideOutOffsetY, _slideOutDurationInSeconds)
                    .SetEase(Ease.InBack)
                    .WithCancellationSafe(_localMoveCancellationTokenSource.Token);
            }
            finally
            {
                Despawn();
            }
        }

        public void OnCreated()
        {
            _starMaterial = _starSpriteRenderer.material;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        private void CancelLocalMoveCancellationTokenSource()
        {
            _localMoveCancellationTokenSource?.Cancel();
            _localMoveCancellationTokenSource?.Dispose();
            _localMoveCancellationTokenSource = null;
        }
        
        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
