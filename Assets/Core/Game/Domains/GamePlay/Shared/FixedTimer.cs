using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared
{
    public class FixedTimer
    {
        private readonly float _fixedDelta;

        private double _accumulator;
        private long _lastTime;

        private readonly Stopwatch _stopwatch;
        private readonly Action _onTickAction;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public float LerpAlpha => (float)_accumulator / _fixedDelta;

        public FixedTimer(int ticksPerSecond, Action onTickAction)
        {
            _fixedDelta = 1.0f / ticksPerSecond;
            _stopwatch = new Stopwatch();
            _onTickAction = onTickAction;
        }

        public void Start()
        {
            _lastTime = 0;
            _accumulator = 0.0;
            _stopwatch.Restart();
            _ = RunTimer();
        }

        public void Stop()
        {
            _stopwatch.Stop();
            _cancellationTokenSource.Cancel();
        }

        private async Awaitable RunTimer()
        {
            while (true)
            {
                var elapsedTicks = _stopwatch.ElapsedTicks;
                _accumulator += (double)(elapsedTicks - _lastTime) / Stopwatch.Frequency;
                _lastTime = elapsedTicks;

                if (_accumulator >= _fixedDelta)
                {
                    _onTickAction();
                    _accumulator -= _fixedDelta;
                }

                await Awaitable.NextFrameAsync(_cancellationTokenSource.Token);
            }
        }
    }
}