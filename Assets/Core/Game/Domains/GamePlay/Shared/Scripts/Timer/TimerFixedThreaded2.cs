using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Shared
{
    public sealed class TimerFixedThreaded2
    {
        private float _fixedDelta;
        private double _accumulator;
        private long _lastTime;

        private readonly Stopwatch _stopwatch;
        private readonly Action _onTickAction;

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _thread;

        private readonly object _lock = new object();
        private readonly string _threadName;

        // Stop() and the timer thread's finally block both try to restore the timer resolution, and whichever runs
        // first must be the only one that releases - otherwise a single Start() is paired with two releases and the
        // process-wide ref count drops the resolution while another timer is still relying on it.
        private int _isTimerResolutionRaised;

        public TimerFixedThreaded2(string threadName, float ticksPerSecond, Action onTickAction)
        {
            _threadName = threadName;
            _fixedDelta = ToFixedDelta(ticksPerSecond);
            _stopwatch = new Stopwatch();
            _onTickAction = onTickAction ?? throw new ArgumentNullException(nameof(onTickAction));
        }

        public void SetTicksPerSecond(float ticksPerSecond)
        {
            _fixedDelta = ToFixedDelta(ticksPerSecond);
        }

        private static float ToFixedDelta(float ticksPerSecond)
        {
            return ticksPerSecond <= 0 ? 0 : 1.0f / ticksPerSecond;
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
                RaiseTimerResolution();

                _thread = new Thread(RunTimer)
                {
                    IsBackground = true,
                    Name = _threadName
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
                ReleaseTimerResolutionIfRaised();
            }
        }

        private void RaiseTimerResolution()
        {
            if (Interlocked.Exchange(ref _isTimerResolutionRaised, 1) == 0)
            {
                WinTime.Begin1ms();
            }
        }

        private void ReleaseTimerResolutionIfRaised()
        {
            if (Interlocked.Exchange(ref _isTimerResolutionRaised, 0) == 1)
            {
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

                    if (_fixedDelta > 0)
                    {
                        while (_accumulator >= _fixedDelta)
                        {
                            _onTickAction();
                            _accumulator -= _fixedDelta;

                            if (token.IsCancellationRequested)
                                break;
                        }
                    }
                    else
                    {
                        _onTickAction();
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
                ReleaseTimerResolutionIfRaised();
            }
        }

        // ------------------------------------------------------------
        // Windows timer resolution helper (timeBeginPeriod / timeEndPeriod)
        // ------------------------------------------------------------
        // WINDOWS is not among the project's scripting define symbols, so guarding on it alone compiled these bodies
        // out of every build - including the dedicated Windows server, which is the one target that needs them. The
        // Unity-provided symbols are the reliable ones. Note the [Conditional] attributes that used to sit on Begin1ms
        // are gone: they strip the call site rather than the body, which only made the raise/release pair asymmetric on
        // non-Windows targets, and stripping a once-per-match call was never worth anything.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || WINDOWS
        private static class WinTime
        {
            // Keep this ref-counted so multiple timers can coexist safely.
            private static int _refCount;

            public static void Begin1ms()
            {
                if (Interlocked.Increment(ref _refCount) == 1)
                {
                    timeBeginPeriod(1);
                }
            }

            public static void End1ms()
            {
                int count = Interlocked.Decrement(ref _refCount);
                if (count == 0)
                {
                    timeEndPeriod(1);
                }

                // If Stop() called more times than Start(), clamp.
                if (count < 0)
                {
                    Interlocked.Exchange(ref _refCount, 0);
                }
            }

            [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", ExactSpelling = true)]
            private static extern uint timeBeginPeriod(uint uMilliseconds);

            [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", ExactSpelling = true)]
            private static extern uint timeEndPeriod(uint uMilliseconds);
        }
#else
        private static class WinTime
        {
            public static void Begin1ms()
            {
            }

            public static void End1ms()
            {
            }
        }
#endif
    }
}
