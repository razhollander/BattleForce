using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreDomain.Scripts.Services.Logger.Base;

public class TimerFixedThreaded
{
    private readonly float _fixedDelta;
    private double _accumulator;
    private long _lastTime;

    private readonly Stopwatch _stopwatch;
    private readonly Action _onTickAction;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _timerTask;

    public float LerpAlpha => (float)_accumulator / _fixedDelta;

    public TimerFixedThreaded(int ticksPerSecond, Action onTickAction)
    {
        _fixedDelta = 1.0f / ticksPerSecond;
        _stopwatch = new Stopwatch();
        _onTickAction = onTickAction;
    }

    public void Start(CancellationTokenSource cancellationTokenSource)
    {
        _cancellationTokenSource = cancellationTokenSource;
        _lastTime = 0;
        _accumulator = 0.0;
        _stopwatch.Restart();
#if Logs
        LogService.LogTopic("start tick", LogTopicType.ServerNetwork);
#endif
        _timerTask = Task.Run(RunTimer, _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _stopwatch.Stop();

        try
        {
            _timerTask?.Wait();
        }
        catch (AggregateException)
        {
        }
    }

    private async Task RunTimer()
    {
        var token = _cancellationTokenSource.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                var elapsedTicks = _stopwatch.ElapsedTicks;
                _accumulator += (double)(elapsedTicks - _lastTime) / Stopwatch.Frequency;
                _lastTime = elapsedTicks;

                while (_accumulator >= _fixedDelta)
                {
                    _onTickAction();
                    _accumulator -= _fixedDelta;
                }

                await Task.Delay(1, token);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }
}