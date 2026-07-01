using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerPowerUpHudView : MonoBehaviour
    {
        private const float REEL_FRAME_INTERVAL_SECONDS = 0.08f;
        private const float GRANTED_GLOW_DURATION_SECONDS = 0.35f;
        private const float GRANTED_GLOW_SCALE = 1.25f;

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
                var frameIndex = 0;
                while (reelSprites.Count > 0)
                {
                    _powerUpImage.sprite = reelSprites[frameIndex % reelSprites.Count];
                    frameIndex++;
                    await Awaitable.WaitForSecondsAsync(REEL_FRAME_INTERVAL_SECONDS, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Awaitable StopGrantingPhaseReelAndShowGranted(Sprite grantedSprite, CancellationToken cancellationToken)
        {
            _grantingPhaseCancellationTokenSource?.Cancel();

            _container.SetActive(true);
            _powerUpImage.sprite = grantedSprite;

            var grantedTransform = _powerUpImage.transform;
            var baseScale = grantedTransform.localScale;
            var elapsed = 0f;

            try
            {
                while (elapsed < GRANTED_GLOW_DURATION_SECONDS)
                {
                    var t = elapsed / GRANTED_GLOW_DURATION_SECONDS;
                    var pulse = Mathf.Sin(t * Mathf.PI);
                    grantedTransform.localScale = baseScale * (1f + pulse * (GRANTED_GLOW_SCALE - 1f));
                    await Awaitable.NextFrameAsync(cancellationToken);
                    elapsed += Time.deltaTime;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                grantedTransform.localScale = baseScale;
            }
        }
    }
}
