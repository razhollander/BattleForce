using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts
{
    public class HitDamageIndicatorEffectController : IHitDamageIndicatorEffectController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly HitDamageIndicatorEffectPool _effectsPool;

        public HitDamageIndicatorEffectController(HitDamageIndicatorEffectView prefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new HitDamageIndicatorEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void PlayEffect(ushort damage, Vector2 position)
        {
            PlayEffectAsync(damage, position, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        private async Awaitable PlayEffectAsync(ushort damage, Vector2 position, CancellationTokenSource cancellationTokenSource)
        {
            var view = _effectsPool.Spawn();
            await view.PlayAndDespawn(damage, position, cancellationTokenSource);
        }
    }
}
