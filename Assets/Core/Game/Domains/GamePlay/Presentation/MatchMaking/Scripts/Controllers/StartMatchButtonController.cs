using System;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Views;
using Core.Scripts.Services.Timer.Scripts;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Controllers
{
    public interface IStartMatchButtonController
    {
        void Initialize();
        void Dispose();
        void OnStartMatchCountdown(float duration);
        void OnStopMatchCountdown();
    }

    public class StartMatchButtonController : IStartMatchButtonController
    {
        private const string TimerLabel = "StartMatchCountdown";
        
        private readonly StartMatchButtonView _viewPrefab;
        private readonly ITimerService _timerService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private StartMatchButtonView _viewInstance;


        public StartMatchButtonController(StartMatchButtonView viewPrefab, ITimerService timerService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _viewPrefab = viewPrefab;
            _timerService = timerService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void Initialize()
        {
            _viewInstance = Object.Instantiate(_viewPrefab);
            _viewInstance.SetPosition(Vector2.zero);
            _viewInstance.SetRadius(_sharedGamePlayConfig.MatchMakingEnvironment.);
            _viewInstance.SetText("Start");
        }

        public void Dispose()
        {
            if (_viewInstance != null)
            {
                Object.Destroy(_viewInstance.gameObject);
            }
        }

        public void OnStartMatchCountdown(float duration)
        {
            var settings = new TimerSettings(TimerLabel, duration, OnTimerTick, OnTimerCompleted, OnTimerCanceled);
            _timerService.StartTimer(settings, default);
        }

        public void OnStopMatchCountdown()
        {
            _timerService.CancelTimer(TimerLabel);
            ResetText();
        }

        private void OnTimerTick(double percent, TimeSpan timeLeft)
        {
            if (_viewInstance != null)
            {
                _viewInstance.SetText(Mathf.CeilToInt((float)timeLeft.TotalSeconds).ToString());
            }
        }

        private void OnTimerCompleted()
        {
            // Optional: Handle completion if needed
        }

        private void OnTimerCanceled()
        {
            ResetText();
        }

        private void ResetText()
        {
             if (_viewInstance != null)
            {
                _viewInstance.SetText("Start");
            }
        }
    }
}
