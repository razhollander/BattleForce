using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc
{
    public class GrapplingHookProjectileView : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _coilWidth = 0.3f;
        [SerializeField] private int _numberOfCoils = 8;
        [SerializeField] private int _pointsPerCoil = 15;
        [SerializeField] private Transform _hookPivot;
        
        private bool _isHookAttached;
        private float _maxDistance;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void Setup(Vector2 hookPosition, Quaternion rotation, Vector2 lineStartPosition, float maxDistance)
        {
            _maxDistance = maxDistance;
            SetTransform(hookPosition, rotation, lineStartPosition);
        }

        public void SetTransform(Vector2 hookPosition, Quaternion rotation, Vector2 lineStartPosition)
        {
            Transform.SetPositionAndRotation(hookPosition, rotation);
            UpdateLineRenderer(_hookPivot.position, lineStartPosition);
        }

        public void SetIsHookAttached(bool isHookAttached)
        {
            _isHookAttached = isHookAttached;
        }

        private void UpdateLineRenderer(Vector2 startPosition, Vector2 endPosition)
        {
            if (_isHookAttached)
            {
                UpdateStriaghtLineRendererPoints(startPosition, endPosition);
            }
            else
            {
                UpdateWavyLineRendererPoints(startPosition, endPosition);
            }
        }

        private void UpdateWavyLineRendererPoints(Vector2 startPosition, Vector2 endPosition)
        {
            float currentDistance = Vector2.Distance(startPosition, endPosition);
            float stretchFactor = Mathf.Clamp01(1f - (currentDistance / _maxDistance));
            var totalPoints = _numberOfCoils * _pointsPerCoil;
            _lineRenderer.positionCount = totalPoints;
            var perpendicular = (Vector2)CoreDomain.Scripts.Utils.MathUtils.GetPerpendicularDirection(startPosition, endPosition);

            for (int i = 0; i < totalPoints; i++)
            {
                var interpolation = (float)i / (totalPoints - 1);
                var basePosition = Vector2.Lerp(startPosition, endPosition, interpolation);
                var currentAngle = interpolation * _numberOfCoils * Mathf.PI * 2f;
                var dynamicCoilWidth = _coilWidth * stretchFactor;
                var sidewaysOffset = perpendicular * Mathf.Sin(currentAngle) * dynamicCoilWidth;
                var finalPosition = basePosition + sidewaysOffset;
                _lineRenderer.SetPosition(i, finalPosition);
            }
        }

        private void UpdateStriaghtLineRendererPoints(Vector2 startPosition, Vector2 endPosition)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, startPosition);
            _lineRenderer.SetPosition(1, endPosition);
        }

        public void OnCreated()
        {
            Transform = transform;
        }

        public void OnSpawned()
        {
            _isHookAttached = false;
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}