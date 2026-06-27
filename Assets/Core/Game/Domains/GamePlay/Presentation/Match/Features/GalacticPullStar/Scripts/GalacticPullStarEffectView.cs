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
        [SerializeField] private float _hiddenLocalY = 6f;
        [SerializeField] private float _slideInDurationInSeconds = 0.35f;
        [SerializeField] private float _slideOutDurationInSeconds = 0.25f;
        [SerializeField] private float _reflowDurationInSeconds = 0.2f;

        private Material _starMaterial;

        public Action Despawn { get; set; }

        public void Setup(Color outlineColor)
        {
            _starMaterial.SetColor(OUTLINE_SHADER_PROPERTY, outlineColor);
        }

        public async Awaitable SlideIn(float targetLocalX, CancellationTokenSource cancellationTokenSource)
        {
            transform.localPosition = new Vector3(targetLocalX, _hiddenLocalY, 0f);
            await transform.DOLocalMove(new Vector3(targetLocalX, 0f, 0f), _slideInDurationInSeconds)
                .SetEase(Ease.OutBack)
                .WithCancellationSafe(cancellationTokenSource.Token);
        }

        public async Awaitable MoveToSlot(float targetLocalX, CancellationTokenSource cancellationTokenSource)
        {
            await transform.DOLocalMoveX(targetLocalX, _reflowDurationInSeconds)
                .SetEase(Ease.OutQuad)
                .WithCancellationSafe(cancellationTokenSource.Token);
        }

        public async Awaitable SlideOut(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                await transform.DOLocalMoveY(_hiddenLocalY, _slideOutDurationInSeconds)
                    .SetEase(Ease.InBack)
                    .WithCancellationSafe(cancellationTokenSource.Token);
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

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
