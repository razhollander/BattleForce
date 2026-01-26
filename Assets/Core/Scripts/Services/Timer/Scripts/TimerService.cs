using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Scripts.Services.Timer.Scripts
{
    public class TimerService : ITimerService
    {
        private readonly Dictionary<string, Timer> _activeTimerDictionary = new();

        public void StartTimer(TimerSettings settings, CancellationToken cancellationToken)
        {
            CancelTimer(settings.Label);
            var timer = new Timer(settings, RemoveActiveTimer, cancellationToken);
            AddActiveTimer(timer);
            _ =timer.StartCountdown();
        }

        public void PauseTimer(string label)
        {
            if (_activeTimerDictionary.TryGetValue(label, out var timer))
            {
                timer.PauseTimer();
            }
        }

        public void ResumeTimer(string label)
        {
            if (_activeTimerDictionary.TryGetValue(label, out var timer))
            {
                timer.ResumeTimer();
            }
        }

        public void CancelTimer(string label)
        {
            if (_activeTimerDictionary.TryGetValue(label, out var timer))
            {
                RemoveActiveTimer(timer);
                timer.CancelTimer();
            }
        }

        public void AddOnTickListener(string label, Action<double, TimeSpan> action, bool invokeImmediately = false)
        {
            if (_activeTimerDictionary.TryGetValue(label, out var timer))
            {
                timer.AddOnTickListener(action, invokeImmediately);
            }
        }
        
        public void RemoveOnTickListener(string label, Action<double, TimeSpan> action)
        {
            if (_activeTimerDictionary.TryGetValue(label, out var timer))
            {
                timer.RemoveOnTickListener(action);
            }
        }
        
        public bool AddOnCompleteListener(string label, Action action)
        {
            if (_activeTimerDictionary.TryGetValue(label, out var timer))
            {
                timer.AddOnCompleteListener(action);
                return true;
            }

            return false;
        }
        
        public void RemoveOnCompleteListener(string label, Action action)
        {
            if (_activeTimerDictionary.TryGetValue(label, out var timer))
            {
                timer.RemoveOnCompleteListener(action);
            }
        }

        public bool HasTimerEnded(string label)
        {
            return !_activeTimerDictionary.ContainsKey(label);
        }

        private void AddActiveTimer(Timer timer)
        {
            _activeTimerDictionary[timer.Label] = timer;
        }
        
        private void RemoveActiveTimer(Timer timer)
        {
            if (_activeTimerDictionary.ContainsKey(timer.Label))
            {
                _activeTimerDictionary.Remove(timer.Label);
            }
        }
    }
}