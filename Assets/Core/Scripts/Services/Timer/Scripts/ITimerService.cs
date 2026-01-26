using System;
using System.Threading;

namespace Core.Scripts.Services.Timer.Scripts
{
    public interface ITimerService
    {
        void StartTimer(TimerSettings settings, CancellationToken cancellationToken);
        void PauseTimer(string label);
        void ResumeTimer(string label);
        void CancelTimer(string label);
        void AddOnTickListener(string label, Action<double, TimeSpan> action, bool invokeImmediately = false);
        void RemoveOnTickListener(string label, Action<double, TimeSpan> action);
        bool AddOnCompleteListener(string label, Action action);
        void RemoveOnCompleteListener(string label, Action action);
        bool HasTimerEnded(string label);
    }
}