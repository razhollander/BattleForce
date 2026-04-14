using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc
{
    public class GrapplingHookProjectileView : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer _lineRenderer;
        
        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void Setup(Vector2 hookPosition, Quaternion rotation, Vector2 lineStartPosition)
        {
            SetTransform(hookPosition, rotation, lineStartPosition);
        }

        public void SetTransform(Vector2 hookPosition, Quaternion rotation, Vector2 lineStartPosition)
        {
            Transform.SetPositionAndRotation(hookPosition, rotation);
            UpdateLineRenderer(hookPosition, lineStartPosition);
        }

        private void UpdateLineRenderer(Vector2 startPosition, Vector2 endPosition)
        {
            _lineRenderer.SetPosition(0, startPosition);
            _lineRenderer.SetPosition(1, endPosition);
        }

        public void UpdateOnHit()
        {
            // Empty method to be implemented later
        }

        public void OnCreated()
        {
            Transform = transform;
            _lineRenderer.positionCount = 2;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
