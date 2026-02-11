using System;
using Core.Scripts.Services.Timer.Scripts;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs
{
    public class StartMatchButtonController : IStartMatchButtonController
    {
        private const string TIMER_LABEL = "StartMatchCountdown";
        
        private readonly StartMatchButtonView _viewPrefab;
        private readonly ITimerService _timerService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly IStateMachineService _stateMachineService;

        private StartMatchButtonView _view;
        
        public StartMatchButtonController(StartMatchButtonView viewPrefab, ITimerService timerService, SharedGamePlayConfig sharedGamePlayConfig, IStateMachineService stateMachineService)
        {
            _viewPrefab = viewPrefab;
            _timerService = timerService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _stateMachineService = stateMachineService;
        }

        public void InitEntryPoint()
        {
            _view = Object.Instantiate(_viewPrefab);
            _view.Setup(Vector2.zero, _sharedGamePlayConfig.MatchMakingEnvironment.StartMatchWallRadius);
            SetButtonStartState();
        }

        public void StartMatchCountdown(float duration)
        {
            var settings = new TimerSettings(TIMER_LABEL, duration, OnTimerTick);
            _view.SetCountdownState();
            _timerService.StartTimer(settings, _stateMachineService.CurrentState().CancellationTokenSource.Token);
        }

        public void StopMatchCountdown()
        {
            _timerService.CancelTimer(TIMER_LABEL);
            SetButtonStartState();
        }

        public void SetIsEnabled(bool isEnabled)
        {
            _view.SetIsDisabledOverlayShown(!isEnabled);
        }

        private void OnTimerTick(double percent, TimeSpan timeLeft)
        {
            _view.SetCountdownText(Mathf.CeilToInt((float) timeLeft.TotalSeconds).ToString());
        }

        private void SetButtonStartState()
        {
            _view.SetStartState();
        }
    }
}
