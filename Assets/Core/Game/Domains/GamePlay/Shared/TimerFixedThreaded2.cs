using System;
using System.Diagnostics;
using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Shared
{
    public sealed class TimerFixedThreaded
    {
        private readonly float _fixedDelta;
        private double _accumulator;
        private long _lastTime;

        private readonly Stopwatch _stopwatch;
        private readonly Action _onTickAction;

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _thread;

        private readonly object _lock = new object();

        public float LerpAlpha => (float)(_accumulator / _fixedDelta);

        public TimerFixedThreaded(int ticksPerSecond, Action onTickAction)
        {
            if (ticksPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticksPerSecond));

            _fixedDelta = 1.0f / ticksPerSecond;
            _stopwatch = new Stopwatch();
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

                _lastTime = 0;
                _accumulator = 0.0;
                _stopwatch.Restart();

                LogService.LogTopic("start tick", LogTopicType.ServerNetwork);

                // ✅ Increase timer resolution on Windows for Sleep(1) accuracy
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
                _stopwatch.Stop();

                if (_thread != null && _thread.IsAlive)
                {
                    try
                    {
                        _thread.Join();
                    }
                    catch (ThreadStateException) { }
                }

                _thread = null;

                // ✅ Restore system timer resolution on Windows
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
                    var elapsedTicks = _stopwatch.ElapsedTicks;
                    _accumulator += (double)(elapsedTicks - _lastTime) / Stopwatch.Frequency;
                    _lastTime = elapsedTicks;

                    while (_accumulator >= _fixedDelta)
                    {
                        _onTickAction();
                        _accumulator -= _fixedDelta;

                        if (token.IsCancellationRequested)
                            break;
                    }

                    // ✅ With timeBeginPeriod(1), this is much more accurate on Windows
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"[TimerFixedThreaded] Timer thread crashed: {ex}");
            }
            finally
            {
                // Safety: if thread exits unexpectedly, ensure we release timer period
                WinTime.End1ms();
            }
        }

        // ------------------------------------------------------------
        // Windows timer resolution helper (timeBeginPeriod / timeEndPeriod)
        // ------------------------------------------------------------
        private static class WinTime
        {
            // Keep this ref-counted so multiple timers can coexist safely.
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

            public static void End1ms()
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || WINDOWS
            int count = Interlocked.Decrement(ref _refCount);
            if (count == 0)
                timeEndPeriod(1);

            // If Stop() called more times than Start(), clamp.
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
