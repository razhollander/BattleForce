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

        private CancellationTokenSource _shakeCancellationTokenSource;
        public Camera Camera => _camera;
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
