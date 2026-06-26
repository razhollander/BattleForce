using System.Collections;
using System.Threading;
using Core.Scripts.Utils;
using Unity.Cinemachine;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    [RequireComponent(typeof(CinemachineTargetGroup))]
    public class WorldCameraView : MonoBehaviour
    {
        [SerializeField] private CinemachineTargetGroup _targetGroup;
        [SerializeField] private CinemachineBasicMultiChannelPerlin _perlin;
        [SerializeField] private Camera _camera;
        [SerializeField] private Camera _baseCamera;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineGroupFraming _cinemachineGroupFraming;
        [SerializeField] private float _deafultOrthographicSize = 30f;

        private CancellationTokenSource _shakeCancellationTokenSource;
        private CancellationTokenSource _zoomCancellationTokenSource;
        public Camera Camera => _camera;
        public Camera BaseCamera => _baseCamera;

        public void MultiplyOthographicSize(float multiplier)
        {
            _zoomCancellationTokenSource?.Cancel();
            _zoomCancellationTokenSource = null;
            var orthoSize = _deafultOrthographicSize * multiplier;
            _cinemachineCamera.Lens.OrthographicSize = orthoSize;
            _cinemachineGroupFraming.OrthoSizeRange.y = orthoSize;
        }

        public void LerpOrthographicSize(float targetMultiplier, float durationSeconds, CancellationTokenSource stateCts)
        {
            _zoomCancellationTokenSource?.Cancel();
            _zoomCancellationTokenSource = new CancellationTokenSource();
            _zoomCancellationTokenSource.CancelWhenTokenCancelled(stateCts.Token);
            LerpOrthographicSizeAsync(targetMultiplier, durationSeconds, _zoomCancellationTokenSource.Token).Forget();
        }

        private async Awaitable LerpOrthographicSizeAsync(float targetMultiplier, float durationSeconds, CancellationToken token)
        {
            var startSize = _cinemachineGroupFraming.OrthoSizeRange.y;
            var targetSize = _deafultOrthographicSize * targetMultiplier;
            var elapsed = 0f;

            while (elapsed < durationSeconds && !token.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                var size = Mathf.Lerp(startSize, targetSize, Mathf.Clamp01(elapsed / durationSeconds));
                _cinemachineGroupFraming.OrthoSizeRange.y = size;
                _cinemachineCamera.Lens.OrthographicSize = size;
                await Awaitable.NextFrameAsync(cancellationToken: default);
            }

            _zoomCancellationTokenSource = null;
        }
        
        public void AddFollowTarget(Transform target, float weight, float radius)
        {
            _targetGroup.AddMember(target, weight, radius);
        }

        public void RemoveFollowTarget(Transform target)
        {
            _targetGroup.RemoveMember(target);
        }

        public void ClearTargets()
        {
            _targetGroup.Targets.Clear();
        }

        public async Awaitable ShakeCamera(float intensity, float durationInSeconds, CancellationTokenSource cancellationTokenSource)
        {
            _shakeCancellationTokenSource?.Cancel();
            _shakeCancellationTokenSource = new CancellationTokenSource();
            _shakeCancellationTokenSource.CancelWhenTokenCancelled(cancellationTokenSource.Token);
            _perlin.AmplitudeGain = intensity;
            await Awaitable.WaitForSecondsAsync(durationInSeconds);
            _perlin.AmplitudeGain = 0f;
            transform.rotation = Quaternion.identity;
            _shakeCancellationTokenSource = null;        
        }

        public Vector3 ScreenToWorldPoint(Vector3 position)
        {
            return _camera.ScreenToWorldPoint(position);
        }
    }
}
