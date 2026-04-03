using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect
{
    public class DashPulseGustEffectController : IDashPulseGustEffectController
    {
        private readonly DashPulseGustEffectPool _effectsPool;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;

        public DashPulseGustEffectController(DashPulseGustEffectView viewPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new DashPulseGustEffectPool(viewPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void PlayEffect(Vector2 position, Vector2 direction)
        {
            PlayEffectAsync(position, direction).Forget();
        }

        private async Awaitable PlayEffectAsync(Vector2 position, Vector2 direction)
        {
            var view = _effectsPool.Spawn();

            try
            {
                await view.PlayGustAnimation(position, direction, _stageCancellationTokenProvider.CancellationTokenSource);
            }
            finally
            {
                view.Despawn();
            }
        }
    }
}
