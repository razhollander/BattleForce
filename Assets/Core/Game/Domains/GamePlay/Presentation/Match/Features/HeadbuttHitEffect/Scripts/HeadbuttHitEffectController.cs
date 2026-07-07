using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts
{
    public class HeadbuttHitEffectController : IHeadbuttHitEffectController
    {
        private readonly HeadbuttHitEffectPool _pool;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;

        public HeadbuttHitEffectController(HeadbuttHitEffectView prefab, DiContainer container, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _pool = new HeadbuttHitEffectPool(prefab, container);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
        }

        public void PlayEffect(Vector2 position)
        {
            var view = _pool.Spawn();
            view.transform.position = new Vector3(position.x, position.y, LayerOrder.Effects);
            view.PlayAndDespawn(_stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }
    }
}
