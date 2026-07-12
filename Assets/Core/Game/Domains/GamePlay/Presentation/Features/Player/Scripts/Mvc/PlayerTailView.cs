using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerTailView : MonoBehaviour
    {
        private static readonly int SPIRAL_SHADER_PROPERTY = Shader.PropertyToID("_SpiralAmount");
        private static readonly int WAVE_AMPLITUDE_SHADER_PROPERTY = Shader.PropertyToID("_WaveAmplitude");
        private static readonly int WAVE_PHASE_SHADER_PROPERTY = Shader.PropertyToID("_WavePhase");
    
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Tail Physics")]
        [Tooltip("How strongly the tail reacts to rotation. Higher = more bend.")]
        [SerializeField] private  float _bendSensitivity = 0.012f;
    
        [Tooltip("Maximum allowed bend to prevent the spiral from breaking or clipping.")]
        [SerializeField] private  float _maxBend = 2f;
    
        [Tooltip("How fast the tail physically snaps to the new bend or returns to straight.")]
        [SerializeField] private  float _tailFlexibility = 10f;
        
        [SerializeField] private float _reachMaxWaveSpeed = 2;

        private float _previousRotationZ;
        private float _currentBend = 0f;
        private Material _tailMaterial;
        private bool _isTailWaving;
        private bool _targetTail;
        private float _maxWaveAmplitude;
        private float _currentWaveAmplitude;
        private float _wavePhase;
        private bool _isTailFrozen;

        public void OnCreated()
        {
            _tailMaterial = _spriteRenderer.material;
            _previousRotationZ = transform.eulerAngles.z;
            _maxWaveAmplitude = _tailMaterial.GetFloat(WAVE_AMPLITUDE_SHADER_PROPERTY);
            _currentWaveAmplitude = _maxWaveAmplitude;
            _isTailFrozen = false;
            _wavePhase = Time.time;
            _tailMaterial.SetFloat(WAVE_PHASE_SHADER_PROPERTY, _wavePhase);
        }

        public void UpdateTail()
        {
            // While frozen we stop driving every tail property so it holds the exact wave pose captured
            // at the moment the freeze started (phase, amplitude and bend all stay put).
            if (_isTailFrozen)
            {
                return;
            }

            AdvanceWavePhase();
            UpdateTailBend();
            UpdateTailWaveAmplitude();
        }

        private void AdvanceWavePhase()
        {
            _wavePhase += Time.deltaTime;
            _tailMaterial.SetFloat(WAVE_PHASE_SHADER_PROPERTY, _wavePhase);
        }

        private void UpdateTailWaveAmplitude()
        {
            var targetWaveAmplitude = _isTailWaving ? _maxWaveAmplitude : 0;
            if (Mathf.Approximately(targetWaveAmplitude, _currentWaveAmplitude))
            {
                return;
            }
            
            _currentWaveAmplitude = Mathf.Lerp(_currentWaveAmplitude, targetWaveAmplitude, Time.deltaTime * _reachMaxWaveSpeed);
            _tailMaterial.SetFloat(WAVE_AMPLITUDE_SHADER_PROPERTY, _currentWaveAmplitude);
        }

        private void UpdateTailBend()
        {
            var currentRotationZ = transform.eulerAngles.z;
            var deltaRotationSinceLastFrame = Mathf.DeltaAngle(_previousRotationZ, currentRotationZ);
            var angularVelocity = deltaRotationSinceLastFrame / Time.deltaTime;
            var targetBend = angularVelocity * -_bendSensitivity; // We multiply by negative sensitivity so the tail lags *behind* the rotation.
            targetBend = Mathf.Clamp(targetBend, -_maxBend, _maxBend);
            _currentBend = Mathf.Lerp(_currentBend, targetBend, Time.deltaTime * _tailFlexibility);
            _tailMaterial.SetFloat(SPIRAL_SHADER_PROPERTY, -_currentBend);
            _previousRotationZ = currentRotationZ;
        }

        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        public void SetIsTailWaving(bool isWaving)
        {
            _isTailWaving = isWaving;
        }

        public void SetIsTailFrozen(bool isFrozen)
        {
            _isTailFrozen = isFrozen;
        }
    }
}