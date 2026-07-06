using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts
{
    public class NukeShockwaveEffectController : INukeShockwaveEffectController
    {
        private readonly NukeShockwaveEffectPool _effectsPool;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;

        public NukeShockwaveEffectController(NukeShockwaveEffectView viewPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new NukeShockwaveEffectPool(viewPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void PlayEffect(Vector2 position)
        {
            PlayEffectAsync(position).Forget();
        }

        private async Awaitable PlayEffectAsync(Vector2 position)
        {
            var view = _effectsPool.Spawn();

            try
            {
                await view.PlayShockwaveAnimation(position, _stageCancellationTokenProvider.CancellationTokenSource);
            }
            finally
            {
                view.Despawn();
            }
        }
    }
}
