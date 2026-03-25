using System;
using System.Threading;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect
{
    public class DashPulseGustEffectView : MonoBehaviour, IPoolable
    {
        public Action Despawn { get; set; }

        public async Awaitable PlayAndDespawn(Vector2 pos, Vector2 direction, float duration, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                transform.position = pos;
                if (direction != Vector2.zero)
                {
                    transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                }

                await Awaitable.WaitForSecondsAsync(duration, cancellationToken: cancellationTokenSource.Token);
            }
            finally
            {
                Despawn();
            }
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
