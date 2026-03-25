using System;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect
{
    public class DashPulseGustEffectController : IDashPulseGustEffectController
    {
        private const float EFFECT_DURATION = 0.5f; // Adjust as needed

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
            _ = PlayEffectAsync(position, direction);
        }

        private async Awaitable PlayEffectAsync(Vector2 position, Vector2 direction)
        {
            var view = _effectsPool.Spawn();

            try
            {
                await view.PlayAndDespawn(position, direction, EFFECT_DURATION, _stateMachineService.CurrentState().CancellationTokenSource);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                LogService.LogError(ex.Message);
            }
        }
    }
}
