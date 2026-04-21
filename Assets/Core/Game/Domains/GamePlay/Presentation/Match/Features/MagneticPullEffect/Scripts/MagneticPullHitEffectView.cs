using System;
using System.Threading;
using Core.Scripts.Helpers;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts
{
    public class MagneticPullHitEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _showDuration = 1f;
        [SerializeField] private SpriteAnimator _spriteAnimator;
        public Action Despawn { get; set; }

        public async Awaitable PlayAndDespawn(Vector2 startPosition, Vector2 endPosition, CancellationTokenSource cancellationTokenSource)
        {
            var direction = (endPosition - startPosition).normalized;
            var distance = Vector2.Distance(startPosition, endPosition);
            var centerPosition = startPosition + direction * (distance * 0.5f);

            transform.position = centerPosition;
            transform.up = direction;
            transform.localScale = new Vector3(1f, distance, 1f);

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