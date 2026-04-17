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
        
        private bool _isAttached;
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

        public void SetIsAttached(bool isAttached)
        {
            _isAttached = isAttached;
        }

        private void UpdateLineRenderer(Vector2 startPosition, Vector2 endPosition)
        {
            float currentDistance = Vector2.Distance(startPosition, endPosition);
            float stretchFactor = Mathf.Clamp01(1f - (currentDistance / _maxDistance));

            if (_isAttached)
            {
                _lineRenderer.positionCount = 2;
                _lineRenderer.SetPosition(0, startPosition);
                _lineRenderer.SetPosition(1, endPosition);
            }
            else
            {
                var totalPoints = _numberOfCoils * _pointsPerCoil;
                _lineRenderer.positionCount = totalPoints;
                var perpendicular = (Vector2)CoreDomain.Scripts.Utils.MathUtils.GetPerpendicularDirection(startPosition, endPosition);

                for (int i = 0; i < totalPoints; i++)
                {
                    float interpolation = (float)i / (totalPoints - 1);
                    var basePosition = Vector2.Lerp(startPosition, endPosition, interpolation);
                    float currentAngle = interpolation * _numberOfCoils * Mathf.PI * 2f;
                    
                    // The spiral also gets thinner as it stretches
                    float dynamicCoilWidth = _coilWidth * stretchFactor;
                    var sidewaysOffset = perpendicular * Mathf.Sin(currentAngle) * dynamicCoilWidth;
                    
                    var finalPosition = basePosition + sidewaysOffset;
                    _lineRenderer.SetPosition(i, finalPosition);
                }
            }
        }

        public void UpdateOnHit()
        {
            SetIsAttached(true);
        }

        public void OnCreated()
        {
            Transform = transform;
        }

        public void OnSpawned()
        {
            _isAttached = false;
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}