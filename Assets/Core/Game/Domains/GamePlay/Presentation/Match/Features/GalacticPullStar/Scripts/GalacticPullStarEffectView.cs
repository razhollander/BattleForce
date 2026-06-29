using System;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

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
        private int _planetBaseSortingOrder;
        private int _gravityForceBaseSortingOrder;

        public Action Despawn { get; set; }

        public void Setup(Color outlineColor, GalacticStarVisualData visualData)
        {
            _starMaterial.SetColor(OUTLINE_SHADER_PROPERTY, outlineColor);
            _starSpriteRenderer.sprite = visualData.PlanetSprite;
            _gravityForceSpriteRenderer.sharedMaterial = visualData.GravityForceMaterial;
        }

        // Draws this star (and its gravity force) above stars with a lower order, keeping each
        // renderer's authored relative offset.
        public void SetSortingOrder(int order)
        {
            _starSpriteRenderer.sortingOrder = _planetBaseSortingOrder + order;
            _gravityForceSpriteRenderer.sortingOrder = _gravityForceBaseSortingOrder + order;
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
            await transform.DOLocalMoveY(targetLocalY, _reflowDurationInSeconds)
                .SetEase(Ease.OutQuad)
                .WithCancellationSafe(cancellationToken);
        }

        public async Awaitable SlideOutAsync(CancellationToken cancellationToken)
        {
            try
            {
                await transform.DOLocalMoveY(transform.localPosition.y - _slideOutOffsetY, _slideOutDurationInSeconds)
                    .SetEase(Ease.InBack)
                    .WithCancellationSafe(cancellationToken);
            }
            finally
            {
                Despawn();
            }
        }

        public void OnCreated()
        {
            _starMaterial = _starSpriteRenderer.material;
            _planetBaseSortingOrder = _starSpriteRenderer.sortingOrder;
            _gravityForceBaseSortingOrder = _gravityForceSpriteRenderer.sortingOrder;
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
