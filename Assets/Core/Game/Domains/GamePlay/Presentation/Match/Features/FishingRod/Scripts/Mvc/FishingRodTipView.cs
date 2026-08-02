using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc
{
    public class FishingRodTipView : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _tipPivot;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void Setup(Vector2 tipPosition, Quaternion rotation, Vector2 lineStartPosition)
        {
            SetTransform(tipPosition, rotation, lineStartPosition);
        }

        public void SetTransform(Vector2 tipPosition, Quaternion rotation, Vector2 lineStartPosition)
        {
            Transform.SetPositionAndRotation(tipPosition, rotation);
            UpdateLineRenderer(lineStartPosition,  _tipPivot.position);
        }

        private void UpdateLineRenderer(Vector2 startPosition, Vector2 endPosition)
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
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
