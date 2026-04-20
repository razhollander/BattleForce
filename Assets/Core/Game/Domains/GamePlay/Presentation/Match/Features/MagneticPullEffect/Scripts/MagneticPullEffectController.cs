using System;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts
{
    public class MagneticPullEffectController : IMagneticPullEffectController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly MagneticPullFieldPool _fieldPool;
        private readonly MagneticPullHitEffectPool _hitEffectPool;

        public MagneticPullEffectController(MagneticPullFieldView fieldPrefab, MagneticPullHitEffectView hitEffectPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _fieldPool = new MagneticPullFieldPool(fieldPrefab, diContainer);
            _hitEffectPool = new MagneticPullHitEffectPool(hitEffectPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _fieldPool.InitPool();
            _hitEffectPool.InitPool();
        }

        public void PlayFieldEffect(Vector2 position, Vector2 rotation, float Radius, Transform parent)
        {
            PlayFieldEffectAsync(position, rotation, Radius, parent, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        public void PlayHitEffect(Vector2 casterPosition, Vector2 enemyPosition, Transform parent)
        {
            PlayHitEffectAsync(casterPosition, enemyPosition, parent, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }

        private async Awaitable PlayFieldEffectAsync(Vector2 position, Vector2 rotation, float radius, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            var view = _fieldPool.Spawn();
            await view.PlayAndDespawn(position, rotation, radius, parent, cancellationTokenSource);
        }

        private async Awaitable PlayHitEffectAsync(Vector2 casterPosition, Vector2 enemyPosition, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            var view = _hitEffectPool.Spawn();
            await view.PlayAndDespawn(casterPosition, enemyPosition, parent, cancellationTokenSource);
        }
    }
}