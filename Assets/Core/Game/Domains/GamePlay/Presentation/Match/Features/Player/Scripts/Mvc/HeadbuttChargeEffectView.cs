using System.Threading;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class HeadbuttChargeEffectView : MonoBehaviour
    {
        private static readonly int SPAWN_INTERVAL_SHADER_PROPERTY = Shader.PropertyToID("_SpawnInterval");
        private static readonly int SHRINK_SPEED_SHADER_PROPERTY = Shader.PropertyToID("_ShrinkSpeed");
        private static readonly int RADIUS_OUTER_SHADER_PROPERTY = Shader.PropertyToID("_RadiusOuter");
        private static readonly int START_COLOR_SHADER_PROPERTY = Shader.PropertyToID("_StartColor");
        private static readonly int END_COLOR_SHADER_PROPERTY = Shader.PropertyToID("_EndColor");
        private static readonly int USE_MANUAL_TIME_SHADER_PROPERTY = Shader.PropertyToID("_UseManualTime");
        private static readonly int ANIM_TIME_SHADER_PROPERTY = Shader.PropertyToID("_AnimTime");

        [SerializeField] private SpriteRenderer _ringsSpriteRenderer;
        [SerializeField] private float _chargeStartSpawnIntervalInSeconds = 0.5f;
        [SerializeField] private float _maxChargeSpawnIntervalInSeconds = 0.15f;
        [SerializeField] private float _chargeStartShrinkSpeed = 0.25f;
        [SerializeField] private float _maxChargeShrinkSpeed = 0.7f;
        [SerializeField] private float _chargeStartOuterRadius = 0.9f;
        [SerializeField] private float _maxChargeOuterRadius = 0.5f;
        [SerializeField] private Color _chargeStartColor;
        [SerializeField] private Color _chargeEndColor;
        [SerializeField] private Color _maxStartColor;
        [SerializeField] private Color _maxEndColor;

        private Material _ringsMaterial;
        private CancellationTokenSource _chargeCancellationTokenSource;
        private float _chargeElapsedInSeconds;
        private float _maxChargeDurationInSeconds;

        public void OnCreated()
        {
            _ringsMaterial = _ringsSpriteRenderer.material;
        }

        public void StartCharging(float maxChargeDurationInSeconds)
        {
            CancelCharging();
            _maxChargeDurationInSeconds = Mathf.Max(maxChargeDurationInSeconds, 0.0001f);
            _chargeElapsedInSeconds = 0f;
            // Ramp the timing params live, so drive the shader from _AnimTime (restarts at 0 each
            // charge) instead of the global _Time — keeps the speed-up reproducible. See shader.
            _ringsMaterial.SetFloat(USE_MANUAL_TIME_SHADER_PROPERTY, 1f);
            gameObject.SetActive(true);
            ApplyCharge();
            _chargeCancellationTokenSource = new CancellationTokenSource();
            AnimateChargeAsync(_chargeCancellationTokenSource.Token).Forget();
        }

        public void StopCharging()
        {
            CancelCharging();
            gameObject.SetActive(false);
        }

        private async Awaitable AnimateChargeAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
                _chargeElapsedInSeconds += Time.deltaTime;
                ApplyCharge();
            }
        }

        private void CancelCharging()
        {
            if (_chargeCancellationTokenSource == null)
            {
                return;
            }

            _chargeCancellationTokenSource.Cancel();
            _chargeCancellationTokenSource.Dispose();
            _chargeCancellationTokenSource = null;
        }

        private void ApplyCharge()
        {
            float chargeFraction = Mathf.Clamp01(_chargeElapsedInSeconds / _maxChargeDurationInSeconds);
            _ringsMaterial.SetFloat(ANIM_TIME_SHADER_PROPERTY, _chargeElapsedInSeconds);
            _ringsMaterial.SetFloat(SPAWN_INTERVAL_SHADER_PROPERTY, Mathf.Lerp(_chargeStartSpawnIntervalInSeconds, _maxChargeSpawnIntervalInSeconds, chargeFraction));
            _ringsMaterial.SetFloat(SHRINK_SPEED_SHADER_PROPERTY, Mathf.Lerp(_chargeStartShrinkSpeed, _maxChargeShrinkSpeed, chargeFraction));
            _ringsMaterial.SetFloat(RADIUS_OUTER_SHADER_PROPERTY, Mathf.Lerp(_chargeStartOuterRadius, _maxChargeOuterRadius, chargeFraction));
            _ringsMaterial.SetColor(START_COLOR_SHADER_PROPERTY, Color.Lerp(_chargeStartColor, _maxStartColor, chargeFraction));
            _ringsMaterial.SetColor(END_COLOR_SHADER_PROPERTY, Color.Lerp(_chargeEndColor, _maxEndColor, chargeFraction));
        }
    }
}
