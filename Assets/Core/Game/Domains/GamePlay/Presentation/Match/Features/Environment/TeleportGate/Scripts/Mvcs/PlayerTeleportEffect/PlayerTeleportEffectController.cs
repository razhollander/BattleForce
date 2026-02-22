using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect
{
    public class PlayerTeleportEffectController : IPlayerTeleportEffectController
    {
        private readonly IStateMachineService _stateMachineService;
        private readonly PlayerTeleportEffectPool _pool;

        public PlayerTeleportEffectController(PlayerTeleportEffectView prefab, DiContainer container, IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
            _pool = new PlayerTeleportEffectPool(prefab, container);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
        }

        public void PlayEffect(Vector2 position)
        {
            var view = _pool.Spawn();
            view.transform.position = new Vector3(position.x, position.y, -5);
            view.PlayAndDespawn(_stateMachineService.CurrentState().CancellationTokenSource).Forget();
        }
    }
}
