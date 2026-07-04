using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Scripts.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerPowerUpHudView : MonoBehaviour
    {
        [SerializeField] float _reelFrameIntervalInSeconds = 0.08f;
        [SerializeField] float _grantedGlowDurationInSeconds = 0.35f;
        [SerializeField] float _grantedGlowScale = 1.25f;
        [SerializeField] private GameObject _container;
        [SerializeField] private Image _powerUpImage;

        private CancellationTokenSource _grantingPhaseCancellationTokenSource;

        public void SetPowerUp(bool isShown, Sprite powerUpSprite)
        {
            _container.SetActive(isShown);

            if (isShown)
            {
                _powerUpImage.sprite = powerUpSprite;
            }
        }

        public async Awaitable PlayGrantingPhaseReel(IReadOnlyList<Sprite> reelSprites, CancellationToken cancellationToken)
        {
            _grantingPhaseCancellationTokenSource?.Cancel();
            _grantingPhaseCancellationTokenSource?.Dispose();
            _grantingPhaseCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _grantingPhaseCancellationTokenSource.Token;

            _container.SetActive(true);

            try
            {
                await SwapQuicklyReelSprites(reelSprites, token);
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
            }
        }

        private async Awaitable SwapQuicklyReelSprites(IReadOnlyList<Sprite> reelSprites, CancellationToken token)
        {
            var frameIndex = 0;

            while (reelSprites.Count > 0)
            {
                _powerUpImage.sprite = reelSprites[frameIndex % reelSprites.Count];
                frameIndex++;
                await Awaitable.WaitForSecondsAsync(_reelFrameIntervalInSeconds, token);
            }
        }

        public async Awaitable StopGrantingPhaseReelAndShowGranted(Sprite grantedSprite, CancellationToken cancellationToken)
        {
            _grantingPhaseCancellationTokenSource?.Cancel();

            _container.SetActive(true);
            _powerUpImage.sprite = grantedSprite;

            var grantedTransform = _powerUpImage.transform;
            var baseScale = grantedTransform.localScale;

            try
            {
                await grantedTransform.DOScale(baseScale * _grantedGlowScale, _grantedGlowDurationInSeconds * 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(2, LoopType.Yoyo)
                    .WithCancellationSafe(cancellationToken);
            }
            finally
            {
                grantedTransform.localScale = baseScale;
            }
        }
    }
}
