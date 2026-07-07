using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts
{
    public class HeadbuttHitEffectController : IHeadbuttHitEffectController
    {
        private readonly IStateMachineService _stateMachineService;
        private readonly HeadbuttHitEffectPool _pool;

        public HeadbuttHitEffectController(HeadbuttHitEffectView prefab, DiContainer container, IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
            _pool = new HeadbuttHitEffectPool(prefab, container);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
        }

        public void PlayEffect(Vector2 position)
        {
            var view = _pool.Spawn();
            view.transform.position = position;
            view.PlayAndDespawn(_stateMachineService.CurrentState().CancellationTokenSource).Forget();
        }
    }
}
