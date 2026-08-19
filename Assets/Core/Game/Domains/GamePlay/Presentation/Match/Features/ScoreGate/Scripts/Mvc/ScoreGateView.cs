using System;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc
{
    public class ScoreGateView : MonoBehaviour, IPoolable
    {
        private const float BEAM_THICKNESS_RELATIVE_TO_POST_WIDTH = 0.28f;

        private const float IDLE_BREATH_DURATION_IN_SECONDS = 1.1f;
        private const float IDLE_BREATH_MIN_ALPHA_RATIO = 0.55f;

        private const float PASS_THICKNESS_PUNCH_MULTIPLIER = 3.6f;
        private const float PASS_LENGTH_OVERSHOOT_MULTIPLIER = 1.06f;
        private const float PASS_THICKNESS_SETTLE_DURATION_IN_SECONDS = 0.7f;
        private const float PASS_LENGTH_SETTLE_DURATION_IN_SECONDS = 0.3f;
        private const float PASS_FLASH_FADE_DURATION_IN_SECONDS = 0.45f;
        private const float PASS_ELASTIC_AMPLITUDE = 1.15f;
        private const float PASS_ELASTIC_PERIOD = 0.35f;

        private const float MULTIPLIER_PUNCH_STRENGTH = 0.6f;
        private const float MULTIPLIER_PUNCH_DURATION_IN_SECONDS = 0.45f;
        private const int MULTIPLIER_PUNCH_VIBRATO = 6;
        private const float MULTIPLIER_PUNCH_ELASTICITY = 0.7f;

        [SerializeField] private Transform _leftPost;
        [SerializeField] private Transform _rightPost;
        [SerializeField] private SpriteRenderer[] _tintableRenderers;
        [SerializeField] private Transform _passLine;
        [SerializeField] private SpriteRenderer _passLineRenderer;
        [SerializeField] private TextMeshPro _multiplierText;

        private Vector3 _baseLineScale = Vector3.one;
        private Color _authoredLineColor = Color.white;
        private Color _restingLineColor = Color.white;
        private Vector3 _multiplierBaseScale = Vector3.one;
        private Quaternion _multiplierAuthoredWorldRotation = Quaternion.identity;
        private Sequence _passSequence;
        private CancellationTokenSource _idleBreathCancellationTokenSource;
        private CancellationTokenSource _passAnimationCancellationTokenSource;
        private CancellationTokenSource _multiplierPunchCancellationTokenSource;

        private bool IsPassAnimationPlaying => _passSequence != null && _passSequence.IsActive();

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void SetTransform(Vector2 position, Quaternion rotation)
        {
            Transform.SetPositionAndRotation(position, rotation);
            KeepMultiplierTextUpright();
        }

        public void SetLayout(Vector2 postSize, float gapWidth)
        {
            PlacePostsOnBothSidesOfGap(postSize, gapWidth);
            StretchBeamAcrossGap(postSize, gapWidth);
            StartIdleBreathing();
        }

        public void SetMultiplierText(string text)
        {
            _multiplierText.text = text;
        }

        public void PlayMultiplierPunch()
        {
            _multiplierPunchCancellationTokenSource = RestartCancellationTokenSource(_multiplierPunchCancellationTokenSource);
            PlayMultiplierPunchAsync(_multiplierPunchCancellationTokenSource.Token).Forget();
        }

        public void SetTeamColor(Color color)
        {
            foreach (var tintableRenderer in _tintableRenderers)
            {
                tintableRenderer.color = color;
            }

            _restingLineColor = WithAuthoredLineAlpha(color);

            if (!IsPassAnimationPlaying)
            {
                _passLineRenderer.color = _restingLineColor;
            }
        }

        public void PlayPassAnimation()
        {
            _passAnimationCancellationTokenSource = RestartCancellationTokenSource(_passAnimationCancellationTokenSource);
            PlayPassAnimationAsync(_passAnimationCancellationTokenSource.Token).Forget();
        }

        public void OnCreated()
        {
            Transform = transform;

            _authoredLineColor = _passLineRenderer.color;
            _restingLineColor = _authoredLineColor;

            var multiplierTransform = _multiplierText.transform;
            _multiplierBaseScale = multiplierTransform.localScale;
            _multiplierAuthoredWorldRotation = multiplierTransform.rotation;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            _restingLineColor = _authoredLineColor;
            _passLineRenderer.color = _authoredLineColor;
        }

        public void OnDespawned()
        {
            CancelIdleBreathing();
            CancelPassAnimation();
            CancelMultiplierPunch();

            _multiplierText.transform.localScale = _multiplierBaseScale;

            gameObject.SetActive(false);
        }

        private async Awaitable PlayPassAnimationAsync(CancellationToken cancellationToken)
        {
            CancelIdleBreathing();
            SnapBeamToFlashedPunchState();

            _passSequence = CreateBeamSettleBackSequence();
            await _passSequence.WithCancellationSafe(cancellationToken);

            StartIdleBreathing();
        }

        private async Awaitable PlayMultiplierPunchAsync(CancellationToken cancellationToken)
        {
            var multiplierTransform = _multiplierText.transform;
            multiplierTransform.localScale = _multiplierBaseScale;

            await multiplierTransform.DOPunchScale(_multiplierBaseScale * MULTIPLIER_PUNCH_STRENGTH,
                    MULTIPLIER_PUNCH_DURATION_IN_SECONDS, MULTIPLIER_PUNCH_VIBRATO, MULTIPLIER_PUNCH_ELASTICITY)
                .WithCancellationSafe(cancellationToken);
        }

        private void StartIdleBreathing()
        {
            _idleBreathCancellationTokenSource = RestartCancellationTokenSource(_idleBreathCancellationTokenSource);
            PlayIdleBreathingAsync(_idleBreathCancellationTokenSource.Token).Forget();
        }

        private async Awaitable PlayIdleBreathingAsync(CancellationToken cancellationToken)
        {
            var restingAlpha = _authoredLineColor.a;
            _passLineRenderer.color = WithAuthoredLineAlpha(_restingLineColor);

            await _passLineRenderer.DOFade(restingAlpha, IDLE_BREATH_DURATION_IN_SECONDS)
                .From(restingAlpha * IDLE_BREATH_MIN_ALPHA_RATIO)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .WithCancellationSafe(cancellationToken);
        }

        private void KeepMultiplierTextUpright()
        {
            _multiplierText.transform.rotation = _multiplierAuthoredWorldRotation;
        }

        private void PlacePostsOnBothSidesOfGap(Vector2 postSize, float gapWidth)
        {
            var postOffsetXFromGapCenter = gapWidth * 0.5f + postSize.x * 0.5f;
            var postScaleForUnitAuthoredSprite = new Vector3(postSize.x, postSize.y, 1f);

            _leftPost.localPosition = new Vector3(-postOffsetXFromGapCenter, 0f, 0f);
            _leftPost.localScale = postScaleForUnitAuthoredSprite;

            _rightPost.localPosition = new Vector3(postOffsetXFromGapCenter, 0f, 0f);
            _rightPost.localScale = postScaleForUnitAuthoredSprite;
        }

        private void StretchBeamAcrossGap(Vector2 postSize, float gapWidth)
        {
            var beamLength = gapWidth;
            var beamThickness = postSize.x * BEAM_THICKNESS_RELATIVE_TO_POST_WIDTH;

            _baseLineScale = new Vector3(beamLength, beamThickness, 1f);
            _passLine.localPosition = Vector3.zero;
            _passLine.localScale = _baseLineScale;
        }

        private void SnapBeamToFlashedPunchState()
        {
            var flashColor = Color.white;
            flashColor.a = 1f;
            _passLineRenderer.color = flashColor;
            _passLine.localScale = new Vector3(_baseLineScale.x * PASS_LENGTH_OVERSHOOT_MULTIPLIER,
                _baseLineScale.y * PASS_THICKNESS_PUNCH_MULTIPLIER, _baseLineScale.z);
        }

        private Sequence CreateBeamSettleBackSequence()
        {
            var settleBackSequence = DOTween.Sequence();

            settleBackSequence.Append(_passLine.DOScaleY(_baseLineScale.y, PASS_THICKNESS_SETTLE_DURATION_IN_SECONDS)
                .SetEase(Ease.OutElastic, PASS_ELASTIC_AMPLITUDE, PASS_ELASTIC_PERIOD));
            settleBackSequence.Join(_passLine.DOScaleX(_baseLineScale.x, PASS_LENGTH_SETTLE_DURATION_IN_SECONDS)
                .SetEase(Ease.OutCubic));
            settleBackSequence.Join(_passLineRenderer.DOColor(_restingLineColor, PASS_FLASH_FADE_DURATION_IN_SECONDS)
                .SetEase(Ease.OutQuad));

            return settleBackSequence;
        }

        private Color WithAuthoredLineAlpha(Color color)
        {
            color.a = _authoredLineColor.a;

            return color;
        }

        private void CancelIdleBreathing()
        {
            CancelAndDisposeCancellationTokenSource(_idleBreathCancellationTokenSource);
            _idleBreathCancellationTokenSource = null;
        }

        private void CancelPassAnimation()
        {
            CancelAndDisposeCancellationTokenSource(_passAnimationCancellationTokenSource);
            _passAnimationCancellationTokenSource = null;
        }

        private void CancelMultiplierPunch()
        {
            CancelAndDisposeCancellationTokenSource(_multiplierPunchCancellationTokenSource);
            _multiplierPunchCancellationTokenSource = null;
        }

        private static CancellationTokenSource RestartCancellationTokenSource(CancellationTokenSource previousCancellationTokenSource)
        {
            CancelAndDisposeCancellationTokenSource(previousCancellationTokenSource);

            return new CancellationTokenSource();
        }

        private static void CancelAndDisposeCancellationTokenSource(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null)
            {
                return;
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
