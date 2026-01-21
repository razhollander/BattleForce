using System;
using System.Threading;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.ObtainedEffect
{
    public class PowerUpBallObtainedEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer _lineRenderer;
        public Action Despawn { get; set; }

        public async Awaitable PlayAndDespawn(Vector2 from, Vector2 to, float duration, CancellationTokenSource cancellationTokenSource)
        {
            _lineRenderer.SetPosition(0, from);
            _lineRenderer.SetPosition(1, to);
            await Awaitable.WaitForSecondsAsync(duration, cancellationToken: cancellationTokenSource.Token);
            Despawn();        
        }

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
    }
}
