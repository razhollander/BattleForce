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
        // Framing (orthographic size) damping, kept separate for zooming out (target grows) and zooming in (target shrinks).
        [SerializeField] private float _deafultZoomOutDamping = 8f;
        [SerializeField] private float _deafultZoomInDamping = 8f;
        [SerializeField] private Ease _zoomEase = Ease.OutCubic;

        private readonly List<CameraTarget> _targets = new List<CameraTarget>();
        private CancellationTokenSource _zoomCancellationTokenSource;
        private Tween _shakeTween;
        private Vector3 _shakeOffset;
        private Vector3 _framedPosition;
        private Vector3 _positionDamping;
        private float _zoomOutDamping;
        private float _zoomInDamping;
        // While true an external zoom animation (LerpOrthographicSize) owns the lens size, so framing leaves it alone.
        private bool _isOrthographicSizeLockedByZoom;
        // When true the camera frustum is kept inside [_worldBoundariesMin, _worldBoundariesMax] every frame.
        private bool _hasWorldBoundaries;
        private Vector2 _worldBoundariesMin;
        private Vector2 _worldBoundariesMax;

        public Camera Camera => _camera;
        public Camera BaseCamera => _baseCamera;

        public void Setup()
        {
            _framedPosition = transform.position;
            _positionDamping = _deafultPositionDamping;
            _zoomOutDamping = _deafultZoomOutDamping;
            _zoomInDamping = _deafultZoomInDamping;
        }

        public void Dispose()
        {
            DisableZoom();
            DisableShake();
            
        }

        private void DisableShake()
        {
            _shakeTween?.Kill();
            _shakeTween = null;
            _shakeOffset = Vector3.zero;
        }

        public void MultiplyOthographicSizeAndDisableZoom(float multiplier)
        {
            DisableZoom();
            _camera.orthographicSize = _deafultOrthographicSize * multiplier;
        }

        private void DisableZoom()
        {
            _zoomCancellationTokenSource?.Cancel();
            _zoomCancellationTokenSource = null;
            _isOrthographicSizeLockedByZoom = false;
        }

        public void SetIsDampingEnabled(bool isEnabled)
        {
            _positionDamping = isEnabled ? _deafultPositionDamping : Vector3.zero;
            _zoomOutDamping = isEnabled ? _deafultZoomOutDamping : 0f;
            _zoomInDamping = isEnabled ? _deafultZoomInDamping : 0f;
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

        // Restricts the camera so its frustum never crosses the rectangle defined by the top-left and bottom-right world points.
        public void SetWorldBoundaries(Vector2 topLeft, Vector2 bottomRight)
        {
            _worldBoundariesMin = new Vector2(topLeft.x, bottomRight.y);
            _worldBoundariesMax = new Vector2(bottomRight.x, topLeft.y);
            _hasWorldBoundaries = true;
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
            DisableShake();
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
                ClampOrthographicSizeToBoundaries();
                _framedPosition = ClampFramedPositionToBoundaries(_framedPosition);
                transform.position = _framedPosition + _shakeOffset;
                return;
            }

            CameraFramingUtils.CalculateTargetsBounds(_targets, out var center, out var extents);

            if (!_isOrthographicSizeLockedByZoom)
            {
                var currentSize = _camera.orthographicSize;
                var targetSize = CameraFramingUtils.CalculateFramedOrthographicSize(extents, _camera.aspect, _percentageKeptFromScreenEdges, _extraPercentageKeptFromScreenBottom, _minOrthographicSize);
                var isZoomingOut = targetSize > currentSize;
                var zoomDamping = isZoomingOut ? _zoomOutDamping : _zoomInDamping;
                _camera.orthographicSize = CameraFramingUtils.Damp(currentSize, targetSize, zoomDamping, deltaTime);
            }

            ClampOrthographicSizeToBoundaries();

            // Move the camera down so the reserved space ends up at the bottom instead of split evenly.
            var bottomOffset = CameraFramingUtils.CalculateBottomWorldOffset(_camera.orthographicSize, _extraPercentageKeptFromScreenBottom);
            _framedPosition.x = CameraFramingUtils.Damp(_framedPosition.x, center.x, _positionDamping.x, deltaTime);
            _framedPosition.y = CameraFramingUtils.Damp(_framedPosition.y, center.y - bottomOffset, _positionDamping.y, deltaTime);
            _framedPosition = ClampFramedPositionToBoundaries(_framedPosition);
            transform.position = _framedPosition + _shakeOffset;
        }

        // Caps the lens so the frustum can never be wider or taller than the world boundaries rectangle.
        private void ClampOrthographicSizeToBoundaries()
        {
            if (!_hasWorldBoundaries)
            {
                return;
            }

            var maxSize = CameraFramingUtils.CalculateMaxOrthographicSizeInBounds(_worldBoundariesMin, _worldBoundariesMax, _camera.aspect);
            if (_camera.orthographicSize > maxSize)
            {
                _camera.orthographicSize = maxSize;
            }
        }

        // Keeps the framed centre inside the world boundaries while preserving the camera's z depth.
        private Vector3 ClampFramedPositionToBoundaries(Vector3 position)
        {
            if (!_hasWorldBoundaries)
            {
                return position;
            }

            var clamped = CameraFramingUtils.ClampPositionToBounds(position, _camera.orthographicSize, _camera.aspect, _worldBoundariesMin, _worldBoundariesMax);
            position.x = clamped.x;
            position.y = clamped.y;
            return position;
        }

        public Vector3 ScreenToWorldPoint(Vector3 position)
        {
            return _camera.ScreenToWorldPoint(position);
        }

        // Setter target for the zoom tween in LerpOrthographicSizeAsync.
        private void SetOrthographicSize(float size)
        {
            _camera.orthographicSize = size;
        }

        private async Awaitable LerpOrthographicSizeAsync(float targetMultiplier, float durationSeconds, CancellationTokenSource cancellationTokenSource)
        {
            var cancellationToken = cancellationTokenSource.Token;
            var targetSize = _deafultOrthographicSize * targetMultiplier;
            _isOrthographicSizeLockedByZoom = true;
            await DOTween.To(() => _camera.orthographicSize, SetOrthographicSize, targetSize, durationSeconds)
                .SetEase(_zoomEase)
                .WithCancellationSafe(cancellationToken);

            SetOrthographicSize(targetSize);

            if (_zoomCancellationTokenSource == cancellationTokenSource)
            {
                _zoomCancellationTokenSource = null;
            }
        }
    }
}
