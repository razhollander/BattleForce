using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public class LockOnTargetShootEffectController : ILockOnTargetShootEffectController
    {
        private readonly IStateMachineService _stateMachineService;
        private readonly LockOnTargetShootEffectPool _effectsPool;

        public LockOnTargetShootEffectController(LockOnTargetShootEffectView prefab, DiContainer diContainer, IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
            _effectsPool = new LockOnTargetShootEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void Play(Vector2 startPosition, Vector2 targetPosition)
        {
            var effectView = _effectsPool.Spawn();
            effectView.Play(startPosition, targetPosition, _stateMachineService.CurrentState().CancellationTokenSource.Token).Forget();
        }
    }
}
