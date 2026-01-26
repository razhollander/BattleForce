using System;

namespace Core.Scripts.Services.Timer.Scripts
{
    public class TimerSettings
    {
        public string Label { get; }
        public double Duration { get; }
        public Action<double, TimeSpan> Tick { get; private set; }
        public Action TimerCompleted { get; private set; }
        public Action TimerCanceled { get; private set; }

        public TimerSettings(string label, double duration, Action<double, TimeSpan> onTick = null, Action onTimerCompleted = null, Action onTimerCanceled = null)
        {
            Label = label;
            Duration = duration;
            Tick = onTick;
            TimerCompleted = onTimerCompleted;
            TimerCanceled = onTimerCanceled;
        }

        public void AddTickAction(Action<double, TimeSpan> action)
        {
            Tick += action;
        }
        
        public void RemoveTickAction(Action<double, TimeSpan> action)
        {
            Tick -= action;
        }
        
        public void AddOnCompleteListener(Action action)
        {
            TimerCompleted += action;
        }
        
        public void RemoveOnCompleteListener(Action action)
        {
            TimerCompleted -= action;
        }
    }
}