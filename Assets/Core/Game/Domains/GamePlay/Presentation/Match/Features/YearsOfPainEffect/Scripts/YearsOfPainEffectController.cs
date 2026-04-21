using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public class YearsOfPainEffectController : IYearsOfPainEffectController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly YearsOfPainViewPool _fieldPool;
        private readonly YearsOfPainHitEffectPool _hitEffectPool;

        public YearsOfPainEffectController(YearsOfPainView fieldPrefab, YearsOfPainHitEffectView hitEffectPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _fieldPool = new YearsOfPainViewPool(new PoolData(3,1), fieldPrefab, diContainer);
            _hitEffectPool = new YearsOfPainHitEffectPool(new PoolData(3,1), hitEffectPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _fieldPool.InitPool();
            _hitEffectPool.InitPool();
        }

        public void PlayFieldEffect(Transform parentTransform, Vector2 direction)
        {
            PlayFieldEffectAsync(parentTransform, direction, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        public void PlayHitEffect(Vector2 position)
        {
            PlayHitEffectAsync(position, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        private async Awaitable PlayFieldEffectAsync(Transform parentTransform, Vector2 direction, CancellationTokenSource cancellationTokenSource)
        {
            var view = _fieldPool.Spawn();
            await view.PlayAndDespawn(parentTransform, direction, cancellationTokenSource);
        }

        private async Awaitable PlayHitEffectAsync(Vector2 position, CancellationTokenSource cancellationTokenSource)
        {
            var view = _hitEffectPool.Spawn();
            await view.PlayAndDespawn(position, cancellationTokenSource);
        }
    }
}
