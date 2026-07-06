using System;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts
{
    public class NukeShockwaveEffectView : MonoBehaviour, IPoolable
    {
        private const string SHOCKWAVE_ANIMATION_NAME = "NukeShockwave";

        [SerializeField] private Animation _animation;
        public Action Despawn { get; set; }

        public async Awaitable PlayShockwaveAnimation(Vector2 position, CancellationTokenSource cancellationTokenSource)
        {
            transform.position = position;
            await _animation.PlayAsync(SHOCKWAVE_ANIMATION_NAME, cancellationToken: cancellationTokenSource.Token);
        }

        public void OnCreated() { }

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
