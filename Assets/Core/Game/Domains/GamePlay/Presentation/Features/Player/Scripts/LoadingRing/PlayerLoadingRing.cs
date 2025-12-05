using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing
{
    public class PlayerLoadingRing : MonoBehaviour
    {
        //private float MAX_SCALE = 1f;
        // private const int ArcEmptyValue = 180;
        // private const int ArcFullValue = 0;
        // private readonly Color PowerUpFullColor = Color.yellow;
        // private readonly Color PowerUpEmptyColor = Color.white;
        // private static readonly int Arc1 = Shader.PropertyToID("_Arc1");
        // private static readonly int Arc2 = Shader.PropertyToID("_Arc2");
    
        [SerializeField] private SpriteRenderer _spriteRenderer;
       // [SerializeField] public float BulletLoadingTime;
       // [SerializeField] public float PowerUpLoadingTime = 10f;
    
     //   public bool IsBulletLoadingReady { get; private set; } = true;
     //   public bool IsPowerUpLoadingReady { get; private set; } = true;

        //private Material _material;
        //private int _currentArcValue = ArcFullValue;
        private float _currentScale;

        public void InitEntryPoint()
        {
           // _material = _spriteRenderer.material;
            //ResetPowerUpLoading();
        }

        
        public void SetRingScale(float scale, float lerpFactor)
        {
            if (Mathf.Approximately(_currentScale,scale))
            {
                return;
            }

            var shouldLerp = scale < _currentScale;
            if (shouldLerp)
            {
                _currentScale = Mathf.Lerp(_currentScale,scale, lerpFactor);
            }
            else
            {
                _currentScale = scale;
            }

            transform.localScale = Vector3.one * _currentScale;
            // if (Mathf.Approximately(scale, MAX_SCALE))
            // {
            //     _spriteRenderer.color = PowerUpFullColor;
            // }
            // else
            // {
            //     _spriteRenderer.color = PowerUpEmptyColor;
            // }
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
