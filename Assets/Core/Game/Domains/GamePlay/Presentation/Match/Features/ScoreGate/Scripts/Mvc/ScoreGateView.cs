using System;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc
{
    // Dumb view: two square posts with an energy beam stretched across the gap between them. The controller drives its
    // transform, layout, team tint, and pass-through juice; the view owns only its own tweens and never talks back.
    public class ScoreGateView : MonoBehaviour, IPoolable
    {
        // The beam is a square sprite stretched thin, so its thickness is authored as a fraction of the post width to
        // stay proportional at every map size.
        private const float BEAM_THICKNESS_RELATIVE_TO_POST = 0.28f;

        // Idle breathing keeps the beam feeling alive between passes: a slow alpha yo-yo down to this fraction and back.
        private const float IDLE_BREATH_DURATION_IN_SECONDS = 1.1f;
        private const float IDLE_BREATH_MIN_ALPHA_RATIO = 0.55f;

        // Pass-through pluck: the beam flashes white, snaps thick, then springs back to base thickness with an elastic
        // wobble while the flash eases back to the beam's colour.
        private const float PASS_THICKNESS_PUNCH_MULTIPLIER = 3.6f;
        private const float PASS_LENGTH_OVERSHOOT_MULTIPLIER = 1.06f;
        private const float PASS_THICKNESS_DURATION_IN_SECONDS = 0.7f;
        private const float PASS_LENGTH_DURATION_IN_SECONDS = 0.3f;
        private const float PASS_COLOR_DURATION_IN_SECONDS = 0.45f;
        private const float PASS_ELASTIC_AMPLITUDE = 1.15f;
        private const float PASS_ELASTIC_PERIOD = 0.35f;

        [SerializeField] private Transform _leftPost;
        [SerializeField] private Transform _rightPost;
        [SerializeField] private SpriteRenderer[] _tintableRenderers;
        [SerializeField] private Transform _passLine;
        [SerializeField] private SpriteRenderer _passLineRenderer;
        [SerializeField] private TextMeshPro _multiplierText;

        private Vector3 _baseLineScale = Vector3.one;
        private Color _neutralLineColor = Color.white;
        private Color _baseLineColor = Color.white;
        private float _baseLineAlpha = 1f;
        private Tween _idleBreathTween;
        private Sequence _passSequence;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void SetTransform(Vector2 position, Quaternion rotation)
        {
            Transform.SetPositionAndRotation(position, rotation);
        }

        // Posts sit on the local X axis at +/-(gap/2 + postHalfWidth). Sprites are assumed authored at 1 unit, so the
        // localScale is the post size directly. The beam spans the gap on the same axis at unit-length scale.x = gapWidth.
        public void SetLayout(Vector2 postSize, float gapWidth)
        {
            var postOffsetX = gapWidth * 0.5f + postSize.x * 0.5f;
            var postScale = new Vector3(postSize.x, postSize.y, 1f);

            if (_leftPost != null)
            {
                _leftPost.localPosition = new Vector3(-postOffsetX, 0f, 0f);
                _leftPost.localScale = postScale;
            }

            if (_rightPost != null)
            {
                _rightPost.localPosition = new Vector3(postOffsetX, 0f, 0f);
                _rightPost.localScale = postScale;
            }

            if (_passLine != null)
            {
                _baseLineScale = new Vector3(gapWidth, postSize.x * BEAM_THICKNESS_RELATIVE_TO_POST, 1f);
                _passLine.localPosition = Vector3.zero;
                _passLine.localScale = _baseLineScale;
            }

            StartIdleBreathing();
        }

        public void SetMultiplierText(string text)
        {
            if (_multiplierText != null)
            {
                _multiplierText.text = text;
            }
        }

        public void SetTeamColor(Color color)
        {
            if (_tintableRenderers != null)
            {
                foreach (var tintableRenderer in _tintableRenderers)
                {
                    tintableRenderer.color = color;
                }
            }

            // The beam adopts the team tint but keeps its own authored transparency, and remembers it as the colour the
            // next pass flash eases back to.
            color.a = _baseLineAlpha;
            _baseLineColor = color;

            // While a pass pluck is mid-flight it owns the colour, so only recolour immediately when it is idle.
            if (_passLineRenderer != null && (_passSequence == null || !_passSequence.IsActive()))
            {
                _passLineRenderer.color = color;
            }
        }

        // The juicy pluck: fired whenever a player passes through the gate. White flash + elastic thickness snap that
        // settles back to the beam's current (team) colour.
        public void PlayPassAnimation()
        {
            if (_passLine == null || _passLineRenderer == null)
            {
                return;
            }

            _idleBreathTween?.Kill();
            _passSequence?.Kill();

            var flashColor = Color.white;
            flashColor.a = 1f;
            _passLineRenderer.color = flashColor;
            _passLine.localScale = new Vector3(_baseLineScale.x * PASS_LENGTH_OVERSHOOT_MULTIPLIER,
                _baseLineScale.y * PASS_THICKNESS_PUNCH_MULTIPLIER, _baseLineScale.z);

            _passSequence = DOTween.Sequence();
            _passSequence.Append(_passLine.DOScaleY(_baseLineScale.y, PASS_THICKNESS_DURATION_IN_SECONDS)
                .SetEase(Ease.OutElastic, PASS_ELASTIC_AMPLITUDE, PASS_ELASTIC_PERIOD));
            _passSequence.Join(_passLine.DOScaleX(_baseLineScale.x, PASS_LENGTH_DURATION_IN_SECONDS)
                .SetEase(Ease.OutCubic));
            _passSequence.Join(_passLineRenderer.DOColor(_baseLineColor, PASS_COLOR_DURATION_IN_SECONDS)
                .SetEase(Ease.OutQuad));
            _passSequence.OnComplete(StartIdleBreathing);
        }

        public void OnCreated()
        {
            Transform = transform;

            if (_passLineRenderer != null)
            {
                _neutralLineColor = _passLineRenderer.color;
                _baseLineColor = _neutralLineColor;
                _baseLineAlpha = _neutralLineColor.a;
            }
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            ResetBeamToNeutral();
        }

        public void OnDespawned()
        {
            _idleBreathTween?.Kill();
            _passSequence?.Kill();
            _idleBreathTween = null;
            _passSequence = null;
            gameObject.SetActive(false);
        }

        private void ResetBeamToNeutral()
        {
            _baseLineColor = _neutralLineColor;
            _baseLineAlpha = _neutralLineColor.a;

            if (_passLineRenderer != null)
            {
                _passLineRenderer.color = _neutralLineColor;
            }
        }

        private void StartIdleBreathing()
        {
            if (_passLineRenderer == null)
            {
                return;
            }

            _idleBreathTween?.Kill();

            var restingColor = _baseLineColor;
            restingColor.a = _baseLineAlpha;
            _passLineRenderer.color = restingColor;

            _idleBreathTween = _passLineRenderer.DOFade(_baseLineAlpha, IDLE_BREATH_DURATION_IN_SECONDS)
                .From(_baseLineAlpha * IDLE_BREATH_MIN_ALPHA_RATIO)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
