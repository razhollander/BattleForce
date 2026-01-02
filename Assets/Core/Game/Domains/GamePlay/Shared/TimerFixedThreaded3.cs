using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Shared
{
    public sealed class TimerFixedThreaded3
    {
        private readonly float _fixedDelta;
        private double _accumulator;
        private long _lastTicksUtc; // DateTime.UtcNow.Ticks

        private readonly Action _onTickAction;

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _thread;

        private readonly object _lock = new object();

        public float LerpAlpha => (float)(_accumulator / _fixedDelta);

        public TimerFixedThreaded3(int ticksPerSecond, Action onTickAction)
        {
            if (ticksPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticksPerSecond));

            _fixedDelta = 1.0f / ticksPerSecond;
            _onTickAction = onTickAction ?? throw new ArgumentNullException(nameof(onTickAction));
        }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null)
                throw new ArgumentNullException(nameof(cancellationTokenSource));

            lock (_lock)
            {
                if (_thread != null && _thread.IsAlive)
                    return;

                _cancellationTokenSource = cancellationTokenSource;

                _accumulator = 0.0;
                _lastTicksUtc = DateTime.UtcNow.Ticks;

                LogService.LogTopic("start tick", LogTopicType.ServerNetwork);

                WinTime.Begin1ms();

                _thread = new Thread(RunTimer)
                {
                    IsBackground = true,
                    Name = "BattleFroce Thread"
                };

                _thread.Start();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_cancellationTokenSource == null)
                    return;

                _cancellationTokenSource.Cancel();

                if (_thread != null && _thread.IsAlive)
                {
                    try
                    {
                        _thread.Join();
                    }
                    catch (ThreadStateException) { }
                }

                _thread = null;

                WinTime.End1ms();
            }
        }

        private void RunTimer()
        {
            var token = _cancellationTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    long nowTicksUtc = DateTime.UtcNow.Ticks;
                    long deltaTicks = nowTicksUtc - _lastTicksUtc;
                    _lastTicksUtc = nowTicksUtc;

                    // DateTime ticks are 100ns => 10,000,000 ticks per second
                    // Convert to seconds:
                    double deltaSeconds = deltaTicks * 1e-7;

                    // ✅ Guard against negative delta if system time jumps backward
                    if (deltaSeconds < 0)
                        deltaSeconds = 0;

                    // ✅ Optional: clamp huge delta if system freezes / debugger pause / time jump forward
                    // This prevents "spiral of death" where it tries to simulate too many ticks at once.
                    if (deltaSeconds > 0.25) // 250ms cap
                        deltaSeconds = 0.25;

                    _accumulator += deltaSeconds;

                    while (_accumulator >= _fixedDelta)
                    {
                        _onTickAction();
                        _accumulator -= _fixedDelta;

                        if (token.IsCancellationRequested)
                            break;
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"[TimerFixedThreaded] Timer thread crashed: {ex}");
            }
            finally
            {
                WinTime.End1ms();
            }
        }

        // ------------------------------------------------------------
        // Windows timer resolution helper (timeBeginPeriod / timeEndPeriod)
        // ------------------------------------------------------------
        private static class WinTime
        {
            private static int _refCount;

            [Conditional("UNITY_STANDALONE_WIN")]
            [Conditional("UNITY_EDITOR_WIN")]
            [Conditional("WINDOWS")]
            public static void Begin1ms()
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || WINDOWS
                if (Interlocked.Increment(ref _refCount) == 1)
                    timeBeginPeriod(1);
#endif
            }

            [Conditional("UNITY_STANDALONE_WIN")]
            [Conditional("UNITY_EDITOR_WIN")]
            [Conditional("WINDOWS")]
            public static void End1ms()
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || WINDOWS
                int count = Interlocked.Decrement(ref _refCount);
                if (count == 0)
                    timeEndPeriod(1);

                if (count < 0)
                    Interlocked.Exchange(ref _refCount, 0);
#endif
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || WINDOWS
            [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", ExactSpelling = true)]
            private static extern uint timeBeginPeriod(uint uMilliseconds);

            [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", ExactSpelling = true)]
            private static extern uint timeEndPeriod(uint uMilliseconds);
#endif
        }
    }
}
