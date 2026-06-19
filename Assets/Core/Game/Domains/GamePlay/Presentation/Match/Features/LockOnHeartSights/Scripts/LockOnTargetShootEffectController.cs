using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetShootEffectController : ILockOnTargetShootEffectController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly LockOnTargetShootEffectPool _effectsPool;

        public LockOnTargetShootEffectController(LockOnTargetShootEffectView prefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new LockOnTargetShootEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void Play(Vector2 casterHeadPosition, Vector2 targetHeartPosition)
        {
            var effectView = _effectsPool.Spawn();
            effectView.Play(casterHeadPosition, targetHeartPosition, _stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }
    }
}
