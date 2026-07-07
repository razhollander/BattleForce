using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class HeadbuttChargeEffectView : MonoBehaviour
    {
        private static readonly int SPAWN_INTERVAL_SHADER_PROPERTY = Shader.PropertyToID("_SpawnInterval");
        private static readonly int SHRINK_SPEED_SHADER_PROPERTY = Shader.PropertyToID("_ShrinkSpeed");
        private static readonly int RADIUS_OUTER_SHADER_PROPERTY = Shader.PropertyToID("_RadiusOuter");

        [SerializeField] private SpriteRenderer _ringsSpriteRenderer;
        [SerializeField] private float _chargeStartSpawnIntervalInSeconds = 0.5f;
        [SerializeField] private float _maxChargeSpawnIntervalInSeconds = 0.15f;
        [SerializeField] private float _chargeStartShrinkSpeed = 0.25f;
        [SerializeField] private float _maxChargeShrinkSpeed = 0.7f;
        [SerializeField] private float _chargeStartOuterRadius = 0.9f;
        [SerializeField] private float _maxChargeOuterRadius = 0.5f;

        private Material _ringsMaterial;
        private Tween _chargeRampTween;

        public void StartCharging(float maxChargeDurationInSeconds)
        {
            EnsureMaterialInstance();
            KillChargeRampTween();
            gameObject.SetActive(true);
            ApplyChargeFraction(0f);
            _chargeRampTween = DOVirtual.Float(0f, 1f, maxChargeDurationInSeconds, ApplyChargeFraction).SetEase(Ease.Linear);
        }

        public void StopCharging()
        {
            KillChargeRampTween();
            gameObject.SetActive(false);
        }

        private void ApplyChargeFraction(float chargeFraction)
        {
            _ringsMaterial.SetFloat(SPAWN_INTERVAL_SHADER_PROPERTY, Mathf.Lerp(_chargeStartSpawnIntervalInSeconds, _maxChargeSpawnIntervalInSeconds, chargeFraction));
            _ringsMaterial.SetFloat(SHRINK_SPEED_SHADER_PROPERTY, Mathf.Lerp(_chargeStartShrinkSpeed, _maxChargeShrinkSpeed, chargeFraction));
            _ringsMaterial.SetFloat(RADIUS_OUTER_SHADER_PROPERTY, Mathf.Lerp(_chargeStartOuterRadius, _maxChargeOuterRadius, chargeFraction));
        }

        private void EnsureMaterialInstance()
        {
            if (_ringsMaterial == null)
            {
                _ringsMaterial = _ringsSpriteRenderer.material;
            }
        }

        private void KillChargeRampTween()
        {
            _chargeRampTween?.Kill();
            _chargeRampTween = null;
        }
    }
}
