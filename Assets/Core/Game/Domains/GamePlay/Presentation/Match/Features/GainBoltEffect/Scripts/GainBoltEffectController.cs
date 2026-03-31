using System;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GainBoltEffect.Scripts
{
    public class GainBoltEffectController : IGainBoltEffectController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly GainBoltEffectPool _effectsPool;
        
        public GainBoltEffectController(GainBoltEffectView prefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new GainBoltEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }
        
        public void PlayEffect(int boltsAmount, Vector2 position, Transform parent)
        {
            PlayEffectAsync(boltsAmount, position, parent, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        private async Awaitable PlayEffectAsync(int boltsAmount, Vector2 position, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            var view = _effectsPool.Spawn();
            await view.PlayAndDespawn(boltsAmount, position, parent, cancellationTokenSource);
        }
    }
}
