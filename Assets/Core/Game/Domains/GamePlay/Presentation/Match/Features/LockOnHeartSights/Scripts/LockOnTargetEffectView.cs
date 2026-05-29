using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer _lineRenderer;

        public Action Despawn { get; set; }

        public void OnCreated()
        {
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }

        public void UpdatePositions(Vector3 headPosition, Vector3 heartPosition)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, headPosition);
                _lineRenderer.SetPosition(1, heartPosition);
            }
        }
    }
}
