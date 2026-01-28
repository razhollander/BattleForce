using System;
using System.Threading;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Scripts.Services.Timer.Scripts
{
    public class Timer
    {
        private readonly TimerSettings _timerSettings;

        public string Label => _timerSettings.Label;
        
        private readonly Action<Timer> _timerCompletedInternal;
        private AwaitableCompletionSource _pauseCompletionSource;
        private CancellationTokenSource _cancellationTokenSource;

        public Timer(TimerSettings settings, Action<Timer> timerCompletedInternal, CancellationToken cancellationToken)
        {
            _timerSettings = settings;
            _timerCompletedInternal = timerCompletedInternal;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }
        
        public async Awaitable StartCountdown()
        {
            float elapsed = 0;
            
            var token = _cancellationTokenSource.Token;

            while (elapsed < _timerSettings.Duration)
            {
                if (token.IsCancellationRequested)
                {
                    _timerSettings.TimerCanceled?.Invoke();
                    return;
                }
                
                if (_pauseCompletionSource != null)
                {
                    try
                    {
                        await _pauseCompletionSource.WithCancellation(token);
                    }
                    catch (OperationCanceledException)
                    {
                        _timerSettings.TimerCanceled?.Invoke();
                        return;
                    }
                    finally
                    {
                        _pauseCompletionSource = null;
                    }
                }

                elapsed += Time.deltaTime;
                var percentCompleted = elapsed / _timerSettings.Duration;
                var timeRemaining = TimeSpan.FromSeconds(_timerSettings.Duration - elapsed);
                _timerSettings.Tick?.Invoke(percentCompleted, timeRemaining);

                await Awaitable.NextFrameAsync(token);
            }

            _timerCompletedInternal?.Invoke(this);
            _timerSettings.TimerCompleted?.Invoke();
        }
        
        public void PauseTimer()
        {
            _pauseCompletionSource = new AwaitableCompletionSource();
        }

        public void ResumeTimer()
        {
            _pauseCompletionSource?.TrySetResult();
        }

        public void AddOnTickListener(Action<double, TimeSpan> action, bool invokeImmediately = false)
        {
            _timerSettings.AddTickAction(action);
            if (invokeImmediately)
            {
                var timeRemaining = TimeSpan.FromSeconds(_timerSettings.Duration);
                action(0, timeRemaining);
            }
        }
        
        public void RemoveOnTickListener(Action<double, TimeSpan> action)
        {
            _timerSettings.RemoveTickAction(action);
        }
        
        public void AddOnCompleteListener(Action action)
        {
            _timerSettings.AddOnCompleteListener(action);
        }
        
        public void RemoveOnCompleteListener(Action action)
        {
            _timerSettings.RemoveOnCompleteListener(action);
        }

        public void CancelTimer()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}