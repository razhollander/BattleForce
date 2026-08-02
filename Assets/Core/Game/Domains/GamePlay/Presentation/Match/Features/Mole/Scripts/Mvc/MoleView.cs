using System;
using System.Threading;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public class MoleView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Animation _animation;
        [SerializeField] private string _spawnAnimationClipName = "MoleSpawn";
        [SerializeField] private string _hitAnimationClipName = "MoleHit";
        [SerializeField] private string _expireAnimationClipName = "MoleExpire";
        [SerializeField] private float _despawnAnimationDurationSeconds = 0.25f;

        public Action Despawn { get; set; }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void PlaySpawnAnimation()
        {
            _animation.Play(_spawnAnimationClipName);
        }

        public async Awaitable PlayHitAndDespawn(CancellationToken cancellationToken)
        {
            await PlayAndDespawn(_hitAnimationClipName, cancellationToken);
        }

        public async Awaitable PlayExpireAndDespawn(CancellationToken cancellationToken)
        {
            await PlayAndDespawn(_expireAnimationClipName, cancellationToken);
        }

        private async Awaitable PlayAndDespawn(string animationClipName, CancellationToken cancellationToken)
        {
            _animation.Play(animationClipName);

            try
            {
                await Awaitable.WaitForSecondsAsync(_despawnAnimationDurationSeconds, cancellationToken);
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
