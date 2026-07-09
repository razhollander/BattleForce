using System;
using System.Collections.Generic;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using DG.Tweening;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public class WorldCameraView : MonoBehaviour
    {
        private const int ShakeVibrato = 10;
        private const float ShakeRandomness = 90f;
        private const float MaxEdgePercentage = 30f;
        private const float PercentToFraction = 0.01f;

        [SerializeField] private Camera _camera;
        [SerializeField] private Camera _baseCamera;
        [SerializeField] private float _deafultOrthographicSize = 30f;
        // Percentage of the screen kept as empty margin between the targets bounding box and the screen edge.
        // 0 = the bounding box's bigger dimension touches the screen edges exactly; 30 = 30% of the screen is left as margin.
        [Range(0f, MaxEdgePercentage)]
        [SerializeField] private float _percentageKeptFromScreenEdges = 10f;
        // Closest the framing is allowed to zoom in, even when the targets are tightly clustered.
        [SerializeField] private float _minOrthographicSize = 10f;
        // Extra empty space reserved at the bottom of the screen only, as a percentage of screen height,
        // on top of the uniform edge margin above. Shifts the framed targets upward.
        [Range(0f, MaxEdgePercentage)]
        [SerializeField] private float _extraPercentageKeptFromScreenBottom;
        [SerializeField] private Vector3 _deafultPositionDamping = new Vector3(10, 10, 10);
        [SerializeField] private float _deafultFramingDamping = 8f;
        [SerializeField] private Ease _zoomEase = Ease.OutCubic;

        private readonly List<CameraTarget> _targets = new List<CameraTarget>();
        private CancellationTokenSource _zoomCancellationTokenSource;
        private Tween _shakeTween;
        private Vector3 _shakeOffset;
        private Vector3 _framedPosition;
        private Vector3 _positionDamping;
        private float _framingDamping;

        public Camera Camera => _camera;
        public Camera BaseCamera => _baseCamera;

        public void Setup()
        {
            _framedPosition = transform.position;
            _positionDamping = _deafultPositionDamping;
            _framingDamping = _deafultFramingDamping;
        }

        public void Cleanup()
        {
            _zoomCancellationTokenSource?.Cancel();
            _zoomCancellationTokenSource = null;
            _shakeTween?.Kill();
            _shakeTween = null;
            _shakeOffset = Vector3.zero;
        }

        public void MultiplyOthographicSize(float multiplier)
        {
            _zoomCancellationTokenSource?.Cancel();
            _zoomCancellationTokenSource = null;
            _camera.orthographicSize = _deafultOrthographicSize * multiplier;
        }

        public void SetIsDampingEnabled(bool isEnabled)
        {
            _positionDamping = isEnabled ? _deafultPositionDamping : Vector3.zero;
            _framingDamping = isEnabled ? _deafultFramingDamping : 0f;
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

        public void AddFollowTarget(Transform target, float radius)
        {
            _targets.Add(new CameraTarget(target, radius));
        }

        public void RemoveFollowTarget(Transform target)
        {
            for (var index = _targets.Count - 1; index >= 0; index--)
            {
                if (_targets[index].Transform == target)
                {
                    _targets.RemoveAt(index);
                    return;
                }
            }
        }

        public void ClearTargets()
        {
            _targets.Clear();
        }

        public void ShakeCamera(float intensity, float durationInSeconds)
        {
            _shakeTween?.Kill();
            _shakeOffset = Vector3.zero;
            _shakeTween = DOTween.Shake(() => _shakeOffset, offset => _shakeOffset = offset, durationInSeconds, intensity, ShakeVibrato, ShakeRandomness, true, true)
                .SetEase(Ease.Linear)
                .OnKill(() => _shakeOffset = Vector3.zero);
        }

        // Frames all follow targets: centres the camera on their combined bounds and zooms to fit them,
        // then applies the current shake offset. Called every late update by the controller.
        public void UpdateFraming(float deltaTime)
        {
            if (_targets.Count == 0)
            {
                transform.position = _framedPosition + _shakeOffset;
                return;
            }

            GetTargetsBounds(out var center, out var extents);

            var targetSize = GetFramedOrthographicSize(extents);
            _camera.orthographicSize = Damp(_camera.orthographicSize, targetSize, _framingDamping, deltaTime);

            // Move the camera down so the reserved space ends up at the bottom instead of split evenly.
            var bottomOffset = _camera.orthographicSize * _extraPercentageKeptFromScreenBottom * PercentToFraction;
            _framedPosition.x = Damp(_framedPosition.x, center.x, _positionDamping.x, deltaTime);
            _framedPosition.y = Damp(_framedPosition.y, center.y - bottomOffset, _positionDamping.y, deltaTime);
            transform.position = _framedPosition + _shakeOffset;
        }

        public Vector3 ScreenToWorldPoint(Vector3 position)
        {
            return _camera.ScreenToWorldPoint(position);
        }

        // Locks the camera to an exact orthographic size by pinning both ends of the range,
        // otherwise the framing re-frames the group and drifts.
        private void SetOrthographicSize(float size)
        {
            _camera.orthographicSize = size;
        }

        private async Awaitable LerpOrthographicSizeAsync(float targetMultiplier, float durationSeconds, CancellationTokenSource cancellationTokenSource)
        {
            var cancellationToken = cancellationTokenSource.Token;
            var targetSize = _deafultOrthographicSize * targetMultiplier;

            await DOTween.To(() => _camera.orthographicSize, SetOrthographicSize, targetSize, durationSeconds)
                .SetEase(_zoomEase)
                .WithCancellationSafe(cancellationToken);

            SetOrthographicSize(targetSize);

            if (_zoomCancellationTokenSource == cancellationTokenSource)
            {
                _zoomCancellationTokenSource = null;
            }
        }

        private void GetTargetsBounds(out Vector2 center, out Vector2 extents)
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            foreach (var target in _targets)
            {
                var position = target.Transform.position;
                var radius = target.Radius;
                min.x = Mathf.Min(min.x, position.x - radius);
                min.y = Mathf.Min(min.y, position.y - radius);
                max.x = Mathf.Max(max.x, position.x + radius);
                max.y = Mathf.Max(max.y, position.y + radius);
            }

            center = (min + max) * 0.5f;
            extents = (max - min) * 0.5f;
        }

        private float GetFramedOrthographicSize(Vector2 extents)
        {
            // The bounding box should occupy this fraction of the screen; the rest is the requested edge margin.
            var boundingBoxScreenFraction = 1f - _percentageKeptFromScreenEdges * PercentToFraction;
            // Vertically, reserve the extra bottom margin too, so the box shrinks to leave room for it.
            var boundingBoxHeightFraction = boundingBoxScreenFraction - _extraPercentageKeptFromScreenBottom * PercentToFraction;
            var sizeForHeight = extents.y / boundingBoxHeightFraction;
            var sizeForWidth = extents.x / _camera.aspect / boundingBoxScreenFraction;
            var desiredSize = Mathf.Max(sizeForHeight, sizeForWidth);
            return Mathf.Max(desiredSize, _minOrthographicSize);
        }

        private static float Damp(float current, float target, float damping, float deltaTime)
        {
            if (damping <= 0f)
            {
                return target;
            }

            var lerpFactor = 1f - Mathf.Exp(-deltaTime / damping);
            return Mathf.Lerp(current, target, lerpFactor);
        }

        private readonly struct CameraTarget
        {
            public readonly Transform Transform;
            public readonly float Radius;

            public CameraTarget(Transform transform, float radius)
            {
                Transform = transform;
                Radius = radius;
            }
        }
    }
}
