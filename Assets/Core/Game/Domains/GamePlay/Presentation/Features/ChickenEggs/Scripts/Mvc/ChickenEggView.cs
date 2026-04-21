using System;
using System.Threading;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc
{
    public class ChickenEggView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Core.Scripts.Helpers.SpriteAnimator _breakAnimator;

        private CancellationTokenSource _breakCancellationTokenSource;
        public Action Despawn { get; set; }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public async Awaitable PlayBreakAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _spriteRenderer.enabled = false;
            _breakAnimator.gameObject.SetActive(true);
            _breakCancellationTokenSource?.Cancel();
            _breakCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            await _breakAnimator.PlayAnimation(_breakCancellationTokenSource);
        }

        public void OnCreated()
        {
            
        }


        public void OnSpawned()
        {
            gameObject.SetActive(true);
            _spriteRenderer.enabled = true;
            _breakAnimator.StopAnimation();
            _breakAnimator.gameObject.SetActive(false);
        }

        public void OnDespawned()
        {
            _breakCancellationTokenSource?.Cancel();
            _breakCancellationTokenSource = null;
            gameObject.SetActive(false);
        }
    }
}
