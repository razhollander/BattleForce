using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect
{
    public class DashPulseGustEffectController : IDashPulseGustEffectController
    {
        private readonly IStateMachineService _stateMachineService;
        private readonly DashPulseGustEffectPool _effectsPool;

        public DashPulseGustEffectController(DashPulseGustEffectView viewPrefab, DiContainer diContainer, IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
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
            await view.PlayGustAnimation(position, direction, _stateMachineService.CurrentState().CancellationTokenSource);
            view.Despawn();
        }
    }
}
