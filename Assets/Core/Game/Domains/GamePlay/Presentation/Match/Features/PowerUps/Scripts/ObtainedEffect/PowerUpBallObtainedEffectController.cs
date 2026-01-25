using System;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.ObtainedEffect
{
    public class PowerUpBallObtainedEffectController : IPowerUpBallObtainedEffectController
    {
        private const float EFFECT_DURATION = 0.2f;
        
        private readonly IStateMachineService _stateMachineService;
        private readonly PowerUpBallObtainedEffectPool _effectsPool;
        
        public PowerUpBallObtainedEffectController(PowerUpBallObtainedEffectView powerUpBallObtainedEffectViewPrefab, DiContainer diContainer, IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
            _effectsPool = new PowerUpBallObtainedEffectPool(powerUpBallObtainedEffectViewPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void PlayEffect(Vector2 from, Vector2 to)
        {
            _ = PlayEffectAsync(from, to);
        }

        private async Awaitable PlayEffectAsync(Vector2 from, Vector2 to)
        {
            var view = _effectsPool.Spawn();

            try
            {
                await view.PlayAndDespawn(from, to, EFFECT_DURATION, _stateMachineService.CurrentState().CancellationTokenSource);
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
