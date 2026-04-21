using System;
using System.Threading;
using Core.Scripts.Helpers;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public class YearsOfPainHitEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _showDuration = 1f;
        [SerializeField] private SpriteAnimator _spriteAnimator;
        public Action Despawn { get; set; }

        public async Awaitable PlayAndDespawn(Vector2 position, CancellationTokenSource cancellationTokenSource)
        {
            transform.position = position;

            try
            {
                _spriteAnimator.PlayAnimation(cancellationTokenSource).Forget();
                await Awaitable.WaitForSecondsAsync(_showDuration, cancellationTokenSource.Token);
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
