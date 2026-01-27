using System;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Views;
using Core.Scripts.Services.Timer.Scripts;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Controllers
{
    public class StartMatchButtonController : IStartMatchButtonController
    {
        private const string TIMER_LABEL = "StartMatchCountdown";
        private const string START_TEXT = "Start";
        
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
            _timerService.StartTimer(settings, _stateMachineService.CurrentState().CancellationTokenSource.Token);
        }

        public void StopMatchCountdown()
        {
            _timerService.CancelTimer(TIMER_LABEL);
            SetButtonStartState();
        }

        private void OnTimerTick(double percent, TimeSpan timeLeft)
        {
            _view.SetText(Mathf.CeilToInt((float) timeLeft.TotalSeconds).ToString());
        }

        private void SetButtonStartState()
        {
            _view.SetText(START_TEXT);
        }
    }
}
