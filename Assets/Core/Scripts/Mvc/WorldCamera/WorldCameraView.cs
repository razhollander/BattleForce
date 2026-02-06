using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    [RequireComponent(typeof(CinemachineTargetGroup))]
    public class WorldCameraView : MonoBehaviour
    {
        [SerializeField] private CinemachineTargetGroup _targetGroup;
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        private CinemachineBasicMultiChannelPerlin _perlin;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (_cinemachineCamera != null)
            {
                _perlin = _cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        public void AddTarget(Transform target, float weight, float radius)
        {
            _targetGroup.AddMember(target, weight, radius);
        }

        public void RemoveTarget(Transform target)
        {
            _targetGroup.RemoveMember(target);
        }

        public void ClearTargets()
        {
            _targetGroup.Targets.Clear();
        }

        public void ShakeCamera(float intensity, float duration)
        {
            if (_perlin == null)
            {
                Debug.LogWarning("CinemachineBasicMultiChannelPerlin component not found on CinemachineCamera.");
                return;
            }

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }

            _shakeCoroutine = StartCoroutine(ShakeCameraCoroutine(intensity, duration));
        }

        private IEnumerator ShakeCameraCoroutine(float intensity, float duration)
        {
            _perlin.AmplitudeGain = intensity;
            yield return new WaitForSeconds(duration);
            _perlin.AmplitudeGain = 0f;
            _shakeCoroutine = null;
        }
    }
}
