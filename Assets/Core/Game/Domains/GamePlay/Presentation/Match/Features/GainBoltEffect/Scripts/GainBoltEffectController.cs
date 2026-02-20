using System;
using System.Threading;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GainBoltEffect.Scripts
{
    public class GainBoltEffectController : IGainBoltEffectController
    {
        private readonly IStateMachineService _stateMachineService;
        private readonly GainBoltEffectPool _effectsPool;
        
        public GainBoltEffectController(GainBoltEffectView prefab, DiContainer diContainer, IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
            _effectsPool = new GainBoltEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }
        
        public void PlayEffect(int boltsAmount, Vector2 position, Transform parent)
        {
            PlayEffectAsync(boltsAmount, position, parent, _stateMachineService.CurrentState().CancellationTokenSource).Forget();
        }

        private async Awaitable PlayEffectAsync(int boltsAmount, Vector2 position, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            var view = _effectsPool.Spawn();
            await view.PlayAndDespawn(boltsAmount, position, parent, cancellationTokenSource);
        }
    }
}
