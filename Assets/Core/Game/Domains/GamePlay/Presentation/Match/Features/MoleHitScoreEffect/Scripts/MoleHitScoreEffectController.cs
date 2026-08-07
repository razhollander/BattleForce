using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MoleHitScoreEffect.Scripts
{
    public class MoleHitScoreEffectController : IMoleHitScoreEffectController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly MoleHitScoreEffectPool _effectsPool;

        public MoleHitScoreEffectController(MoleHitScoreEffectView prefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new MoleHitScoreEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void PlayEffect(byte gainedScore, Vector2 position)
        {
            PlayEffectAsync(gainedScore, position, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        private async Awaitable PlayEffectAsync(byte gainedScore, Vector2 position, CancellationTokenSource cancellationTokenSource)
        {
            var view = _effectsPool.Spawn();
            await view.PlayAndDespawn(gainedScore, position, cancellationTokenSource);
        }
    }
}
