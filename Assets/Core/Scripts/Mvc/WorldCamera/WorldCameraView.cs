using System;
using System.Collections;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using DG.Tweening;
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
        [SerializeField] private CinemachinePositionComposer _cinemachinePositionComposer;
        [SerializeField] private float _deafultOrthographicSize = 30f;
        [SerializeField] private float _deafultGroupFramingDamping = 8;
        [SerializeField] private Vector3 _deafultPositionComposerDamping = new Vector3(10,10,10);
        [SerializeField] private Ease _zoomEase = Ease.OutCubic;

        private CancellationTokenSource _shakeCancellationTokenSource;
        private CancellationTokenSource _zoomCancellationTokenSource;
        public Camera Camera => _camera;
        public Camera BaseCamera => _baseCamera;

        public void MultiplyOthographicSize(float multiplier)
        {
            _zoomCancellationTokenSource?.Cancel();
            _zoomCancellationTokenSource = null;
            // Only the max is set here so the group-framing extension can still dynamically zoom in
            // (down to OrthoSizeRange.x) to frame clustered players during gameplay.
            var range = _cinemachineGroupFraming.OrthoSizeRange;
            range.y = _deafultOrthographicSize * multiplier;
            _cinemachineGroupFraming.OrthoSizeRange = range;
            _cinemachineCamera.Lens.OrthographicSize = _deafultOrthographicSize * multiplier;
        }

        // Locks the camera to an exact orthographic size. The CinemachineGroupFraming extension re-drives
        // Lens.OrthographicSize every frame toward clamp(groupFramedHeight, OrthoSizeRange.x, OrthoSizeRange.y),
        // so we pin both ends of the range to the same value; otherwise the extension re-frames the group and drifts.
        private void SetOrthographicSize(float size)
        {
            var range = _cinemachineGroupFraming.OrthoSizeRange;
            range.x = size;
            range.y = size;
            _cinemachineGroupFraming.OrthoSizeRange = range;
            _cinemachineCamera.Lens.OrthographicSize = size;
        }

        public void SetIsDampingEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                _cinemachinePositionComposer.Damping = _deafultPositionComposerDamping;
                _cinemachineGroupFraming.Damping = _deafultGroupFramingDamping;
            }
            else
            {
                _cinemachinePositionComposer.Damping = Vector3.zero;
                _cinemachineGroupFraming.Damping = 0f;
            }
        }
        
        public async Awaitable LerpOrthographicSize(float targetMultiplier, float durationSeconds, CancellationToken cancellationToken)
        {
            _zoomCancellationTokenSource?.Cancel();
            _zoomCancellationTokenSource = new CancellationTokenSource();
            _zoomCancellationTokenSource.CancelWhenTokenCancelled(cancellationToken);

            try
            {
                await LerpOrthographicSizeAsync(targetMultiplier, durationSeconds, _zoomCancellationTokenSource);
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
            }
        }

        private async Awaitable LerpOrthographicSizeAsync(float targetMultiplier, float durationSeconds, CancellationTokenSource cancellationTokenSource)
        {
            var cancellationToken = cancellationTokenSource.Token;
            var startSize = _cinemachineGroupFraming.OrthoSizeRange.y;
            var targetSize = _deafultOrthographicSize * targetMultiplier;

            await DOTween.To(() => startSize, SetOrthographicSize, targetSize, durationSeconds)
                .SetEase(_zoomEase)
                .WithCancellationSafe(cancellationToken);

            SetOrthographicSize(targetSize);

            if (_zoomCancellationTokenSource == cancellationTokenSource)
            {
                _zoomCancellationTokenSource = null;
            }
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
