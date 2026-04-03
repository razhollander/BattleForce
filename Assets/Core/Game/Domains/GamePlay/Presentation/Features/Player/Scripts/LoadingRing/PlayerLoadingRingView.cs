using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing
{
    public class PlayerLoadingRingView : MonoBehaviour
    {
        private const float MAX_RATIO = 1f;
        private const int ArcEmptyValue = 180;
        private const int ArcFullValue = 0;
        [SerializeField] private Color _talentFullColor = Color.yellow;
        [SerializeField] private Color _talentEmptyColor = Color.white;
        [SerializeField] private float _talentFullThickness = 0.09f;
        [SerializeField] private float _talentEmptyThickness = 0.062f;
        private static readonly int Arc = Shader.PropertyToID("_HalfArc");
        private static readonly int ColorShader = Shader.PropertyToID("_Color");
        private static readonly int Thickness = Shader.PropertyToID("_Thickness");
        //private static readonly int Arc2 = Shader.PropertyToID("_Arc2");

        [SerializeField] private SpriteRenderer _spriteRenderer;
       // [SerializeField] public float BulletLoadingTime;
       // [SerializeField] public float PowerUpLoadingTime = 10f;

     //   public bool IsBulletLoadingReady { get; private set; } = true;
     //   public bool IsPowerUpLoadingReady { get; private set; } = true;

        private Material _material;
        //private int _currentArcValue = ArcFullValue;
        private float _currentScale = 1;

        public void InitEntryPoint()
        {
            _material = _spriteRenderer.material;
            //ResetPowerUpLoading();
        }

        public void SetTalentRingArc(float cooldownLeft, float maxCooldown)
        {
            if (_material == null) return;

            float ratio = Mathf.Clamp01(cooldownLeft / maxCooldown);
            int arcValue = Mathf.RoundToInt(Mathf.Lerp(ArcFullValue, ArcEmptyValue, ratio));

            _material.SetFloat(Arc, arcValue);
            //LogService.LogError($"scale: {scale}");
            LogService.LogError($"cooldownLeft: {cooldownLeft}");
            if (Mathf.Approximately(cooldownLeft, 0))
            {
                
               // _spriteRenderer.color = _talentFullColor;
                _material.SetFloat(Thickness, _talentFullThickness);
                _material.SetColor(ColorShader, _talentFullColor);
            }
            else
            {
                //_spriteRenderer.color = _talentEmptyColor;
                _material.SetFloat(Thickness, _talentEmptyThickness);
                _material.SetColor(ColorShader, _talentEmptyColor);

            }
        }


        public void SetRingScale(float scale, float decay)
        {
            if (Mathf.Approximately(_currentScale,scale))
            {
                return;
            }

            var shouldLerp = scale < _currentScale;
            if (shouldLerp)// todo: bad, this isn't the view responsibility
            {
                _currentScale = MathUtils.ExpDecay(_currentScale,scale, decay, Time.deltaTime);
            }
            else
            {
                _currentScale = scale;
            }

            transform.localScale = Vector3.one * _currentScale;
        }

        // public void DoBulletLoading(TweenCallback onComplete)
        // {
        //     IsBulletLoadingReady = false;
        //     transform.DOScale(Vector3.zero, BulletLoadingTime).OnComplete(OnComplete);
        //
        //     void OnComplete()
        //     {
        //         onComplete();
        //         ResetBulletLoading();
        //     }
        // }

        // public void DoPowerUpLoading()
        // {
        //     IsPowerUpLoadingReady = false;
        //     _currentArcValue = ArcEmptyValue;
        //     _spriteRenderer.color = PowerUpEmptyColor;
        //
        //     DOTween.To(() => _currentArcValue, SetArcValue, ArcFullValue, PowerUpLoadingTime).OnComplete(OnComplete);
        //
        //     void OnComplete()
        //     {
        //         ResetPowerUpLoading();
        //     }
        // }
        //
        // private void ResetPowerUpLoading()
        // {
        //     IsPowerUpLoadingReady = true;
        //     SetArcValue(ArcFullValue);
        //     _spriteRenderer.color = PowerUpFullColor;
        // }
        //
        // private void SetArcValue(int value)
        // {
        //     _currentArcValue = value;
        //     _material.SetFloat(Arc1, value);
        //     _material.SetFloat(Arc2, value);
        // }
        //
        // private void ResetBulletLoading()
        // {
        //     transform.localScale = Vector3.one;
        //     IsBulletLoadingReady = true;
        // }
    }
}
