using System;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect
{
    public class DashPulseGustEffectView : MonoBehaviour, IPoolable
    {
        private const string GUST_ANIMATION_NAME = "DashPulseGust";
        
        [SerializeField] private Animation _animation;
        public Action Despawn { get; set; }

        public async Awaitable PlayGustAnimation(Vector2 pos, Vector2 direction, CancellationTokenSource cancellationTokenSource)
        {
            transform.position = pos;
            transform.rotation = direction.ToQuaternion();
            await _animation.PlayAsync(GUST_ANIMATION_NAME, cancellationToken: cancellationTokenSource.Token);
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
