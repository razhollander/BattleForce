using System;
using System.Threading;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public class LockOnTargetShootEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _laserDurationInSeconds = 0.3f;

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

        public async Awaitable Play(Vector2 startPosition, Vector2 targetPosition, CancellationToken cancellationToken)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, startPosition);
            _lineRenderer.SetPosition(1, targetPosition);
            try
            {
                await Awaitable.WaitForSecondsAsync(_laserDurationInSeconds, cancellationToken);
            }
            finally
            {
                Despawn();
            }
        }
    }
}
