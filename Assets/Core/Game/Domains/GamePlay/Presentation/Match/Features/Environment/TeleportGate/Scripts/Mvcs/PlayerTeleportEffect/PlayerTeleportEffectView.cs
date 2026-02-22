using System;
using System.Collections;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect
{
    public class PlayerTeleportEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _showDuration = 0.3f;
        [SerializeField] private Vector2 _startScale = new Vector2(0.2f, 1f);
        [SerializeField] private Vector2 _endScale = new Vector2(0.5f, 1f);
        private CancellationTokenSource _animationCancellationTokenSource;

        public Action Despawn { get; set; }
        
        public async Awaitable PlayAndDespawn(CancellationTokenSource cancellationTokenSource)
        {
            _animationCancellationTokenSource?.Cancel();
            _animationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            transform.localScale = Vector3.one * _startScale;
            await transform.DOScale(_endScale, _showDuration)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .WithCancellationSafe(cancellationTokenSource.Token);
            Despawn.Invoke();
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
