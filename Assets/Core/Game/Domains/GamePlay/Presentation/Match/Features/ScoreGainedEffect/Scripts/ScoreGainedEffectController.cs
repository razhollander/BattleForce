using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGainedEffect.Scripts
{
    public class ScoreGainedEffectController : IScoreGainedEffectController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly ScoreGainedEffectPool _effectsPool;

        public ScoreGainedEffectController(ScoreGainedEffectView prefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new ScoreGainedEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void PlayEffect(byte gainedScore, Vector2 position, Color? outlineAndUnderlineColor = null)
        {
            PlayEffectAsync(gainedScore, position, outlineAndUnderlineColor, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        private async Awaitable PlayEffectAsync(byte gainedScore, Vector2 position, Color? outlineAndUnderlineColor, CancellationTokenSource cancellationTokenSource)
        {
            var view = _effectsPool.Spawn();
            await view.PlayAndDespawn(gainedScore, position, outlineAndUnderlineColor, cancellationTokenSource);
        }
    }
}
