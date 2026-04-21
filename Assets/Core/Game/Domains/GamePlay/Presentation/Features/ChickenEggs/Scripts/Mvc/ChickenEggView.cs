using System;
using System.Threading;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc
{
    public class ChickenEggView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _eggSpriteRenderer;
        [SerializeField] private SpriteRenderer _borkenEggSpriteRenderer;
        [SerializeField] private float _brokenDurationInSeconds;

        private CancellationTokenSource _breakCancellationTokenSource;
        public Action Despawn { get; set; }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public async Awaitable PlayBreakAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _eggSpriteRenderer.enabled = false;
            _borkenEggSpriteRenderer.enabled = true;
            _breakCancellationTokenSource?.Cancel();
            _breakCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            await Awaitable.WaitForSecondsAsync(_brokenDurationInSeconds, cancellationTokenSource.Token);
        }

        public void OnCreated()
        {
            
        }


        public void OnSpawned()
        {
            gameObject.SetActive(true);
            _eggSpriteRenderer.enabled = true;
        }

        public void OnDespawned()
        {
            _breakCancellationTokenSource?.Cancel();
            _breakCancellationTokenSource = null;
            gameObject.SetActive(false);
        }
    }
}
