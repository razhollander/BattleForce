using System;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect
{
    public class TalentCardObtainedEffectController : ITalentCardObtainedEffectController
    {
        private const float EFFECT_DURATION = 0.2f;
        
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly TalentCardObtainedEffectPool _effectsPool;
        
        public TalentCardObtainedEffectController(TalentCardObtainedEffectView talentCardObtainedEffectViewPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new TalentCardObtainedEffectPool(talentCardObtainedEffectViewPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void InitExitPoint()
        {
            _effectsPool.DisposePool();
        }

        public void PlayEffect(Vector2 from, Vector2 to)
        {
            PlayEffectAsync(from, to).Forget();
        }

        private async Awaitable PlayEffectAsync(Vector2 from, Vector2 to)
        {
            var view = _effectsPool.Spawn();
            await view.PlayAndDespawn(from, to, EFFECT_DURATION, _stageCancellationTokenProvider.CancellationTokenSource);
        }
    }
}
