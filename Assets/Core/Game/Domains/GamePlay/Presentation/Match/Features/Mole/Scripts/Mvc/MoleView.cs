using System;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.Simple_Health_Bar.Scripts;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public class MoleView : MonoBehaviour, IPoolable
    {
        private const float OUTSIDE_HOLE_BODY_LOCAL_POSITION_Y = 0f;

        [SerializeField] private Transform _bodyTransform;
        [SerializeField] private SpriteRenderer _bodySpriteRenderer;
        [SerializeField] private SpriteRenderer _handsSpriteRenderer;

        [Header("Sprites")]
        [SerializeField] private Sprite _idleBodySprite;
        [SerializeField] private Sprite _idleHandsSprite;
        [SerializeField] private Sprite _hitBodySprite;
        [SerializeField] private Sprite _hitHandsSprite;
        [SerializeField] private Sprite _hitWhamBodySprite;

        [Header("Golden Sprites")]
        [SerializeField] private Sprite _goldenIdleBodySprite;
        [SerializeField] private Sprite _goldenIdleHandsSprite;
        [SerializeField] private Sprite _goldenHitBodySprite;
        [SerializeField] private Sprite _goldenHitHandsSprite;
        [SerializeField] private Sprite _goldenHitWhamBodySprite;

        [Header("Health Bar")]
        [SerializeField] private SimpleHealthBar _healthBar;
        [SerializeField] private Canvas _healthBarCanvas;

        [Header("Motion")]
        [SerializeField] private float _inHoleBodyLocalPositionY = -1.6f; // deep enough for the mask to cut the whole body away
        [SerializeField] private float _emergeDurationSeconds = 0.4f;
        [SerializeField] private Ease _emergeEase = Ease.OutBack;
        [SerializeField] private float _hideDurationSeconds = 0.35f;
        [SerializeField] private Ease _hideEase = Ease.InBack;
        [SerializeField] private float _hitWhamDurationSeconds = 0.5f;

        [Header("Hole Shake")]
        [SerializeField] private Vector3 _holeShakeStrength = new Vector3(0.25f, 0.08f, 0f);
        [SerializeField] private int _holeShakeVibrato = 20;
        [SerializeField] private float _holeShakeRandomness = 20f;

        private Tween _bodyTween;
        private Tween _shakeTween;
        private Vector3 _positionBeforeShake;
        private bool _isGolden;


        public Action Despawn { get; set; }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void SetIsGolden(bool isGolden)
        {
            _isGolden = isGolden;
        }

        public void ShowHealthBar(int remainingLives, int maxLives)
        {
            _healthBar.UpdateBar(remainingLives, maxLives, CancellationToken.None);
            SetIsHealthBarShown(true);
        }

        public void UpdateHealthBar(int remainingLives, int maxLives, CancellationToken cancellationToken)
        {
            _healthBar.UpdateBar(remainingLives, maxLives, cancellationToken);
        }

        private void SetIsHealthBarShown(bool isShown)
        {
            _healthBarCanvas.enabled = isShown;
        }

        public void ShowInHoleImmediately()
        {
            KillBodyTween();
            KillShakeTween();
            SetIsHealthBarShown(false);
            SetIdleSprites();
            _handsSpriteRenderer.enabled = false;
            SetBodyLocalPositionY(_inHoleBodyLocalPositionY);
        }

        public void ShowOutsideHoleImmediately()
        {
            KillBodyTween();
            KillShakeTween();
            SetIdleSprites();
            _handsSpriteRenderer.enabled = true;
            SetBodyLocalPositionY(OUTSIDE_HOLE_BODY_LOCAL_POSITION_Y);
        }

        // The whole mole is shaken, so the hole shakes while the mole itself is still hidden inside it.
        public async Awaitable PlayHoleShakeAsync(float shakeDurationSeconds, CancellationToken cancellationToken)
        {
            await PlayShakeAsync(shakeDurationSeconds, cancellationToken);
        }

        // The mole is already out of its hole here, it shakes in place to warn it is about to leave before it drops back in.
        public async Awaitable PlayShakeInPlaceAsync(float shakeDurationSeconds, CancellationToken cancellationToken)
        {
            await PlayShakeAsync(shakeDurationSeconds, cancellationToken);
        }

        private async Awaitable PlayShakeAsync(float shakeDurationSeconds, CancellationToken cancellationToken)
        {
            KillShakeTween();
            _positionBeforeShake = transform.position;
            _shakeTween = transform.DOShakePosition(shakeDurationSeconds, _holeShakeStrength, _holeShakeVibrato, _holeShakeRandomness, fadeOut: false);

            try
            {
                await _shakeTween.WithCancellationSafe(cancellationToken);
            }
            finally
            {
                transform.position = _positionBeforeShake;
            }
        }

        // The hands are only shown once the body has reached the top, they are the only part that is drawn over the dirt.
        public async Awaitable PlayEmergeFromHoleAsync(CancellationToken cancellationToken)
        {
            SetIdleSprites();
            _handsSpriteRenderer.enabled = false;
            var emergeTween = CreateBodyMoveTween(OUTSIDE_HOLE_BODY_LOCAL_POSITION_Y, _emergeDurationSeconds, _emergeEase);
            emergeTween.OnComplete(ShowHands); // an interrupted emerge kills the tween, so the hands only show on a finished one
            await emergeTween.WithCancellationSafe(cancellationToken);
        }

        public async Awaitable PlayHideInHoleAsync(CancellationToken cancellationToken)
        {
            SetIsHealthBarShown(false);
            _handsSpriteRenderer.enabled = false;
            var hideTween = CreateBodyMoveTween(_inHoleBodyLocalPositionY, _hideDurationSeconds, _hideEase);
            await hideTween.WithCancellationSafe(cancellationToken);
        }

        public async Awaitable PlayHitAsync(CancellationToken cancellationToken)
        {
            KillBodyTween();
            KillShakeTween();
            SetIsHealthBarShown(false);
            SetBodyLocalPositionY(OUTSIDE_HOLE_BODY_LOCAL_POSITION_Y); // the mole may still have been emerging when it was hit
            _bodySpriteRenderer.sprite = _isGolden ? _goldenHitWhamBodySprite : _hitWhamBodySprite;
            _handsSpriteRenderer.sprite = _isGolden ? _goldenHitHandsSprite : _hitHandsSprite;
            _handsSpriteRenderer.enabled = true;

            await Awaitable.WaitForSecondsAsync(_hitWhamDurationSeconds, cancellationToken);

            _bodySpriteRenderer.sprite = _isGolden ? _goldenHitBodySprite : _hitBodySprite;
            await PlayHideInHoleAsync(cancellationToken);
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
            KillBodyTween();
            KillShakeTween();
            gameObject.SetActive(false);
        }

        private Tween CreateBodyMoveTween(float localPositionY, float durationSeconds, Ease ease)
        {
            KillBodyTween();
            _bodyTween = _bodyTransform.DOLocalMoveY(localPositionY, durationSeconds).SetEase(ease);
            return _bodyTween;
        }

        private void ShowHands()
        {
            _handsSpriteRenderer.enabled = true;
        }

        private void SetIdleSprites()
        {
            _bodySpriteRenderer.sprite = _isGolden ? _goldenIdleBodySprite : _idleBodySprite;
            _handsSpriteRenderer.sprite = _isGolden ? _goldenIdleHandsSprite : _idleHandsSprite;
        }

        private void SetBodyLocalPositionY(float localPositionY)
        {
            var localPosition = _bodyTransform.localPosition;
            localPosition.y = localPositionY;
            _bodyTransform.localPosition = localPosition;
        }

        private void KillBodyTween()
        {
            if (_bodyTween != null && _bodyTween.IsActive())
            {
                _bodyTween.Kill();
            }
        }

        private void KillShakeTween()
        {
            if (_shakeTween != null && _shakeTween.IsActive())
            {
                _shakeTween.Kill();
                transform.position = _positionBeforeShake;
            }
        }
    }
}
