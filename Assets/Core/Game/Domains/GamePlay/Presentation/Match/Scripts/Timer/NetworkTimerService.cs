using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public class NetworkTimerService : INetworkTimerService
    {
        private readonly ITickCounterService _tickCounterService;
        private readonly NetworkConfig _networkConfig;
        private readonly Dictionary<string, int> _timers = new Dictionary<string, int>();

        public NetworkTimerService(ITickCounterService tickCounterService, NetworkConfig networkConfig)
        {
            _tickCounterService = tickCounterService;
            _networkConfig = networkConfig;
        }

        public string StartTimer(int initialTick)
        {
            var guid = Guid.NewGuid().ToString();
            _timers.Add(guid, initialTick);
            return guid;
        }

        public void CancelTimer(string timerGuid)
        {
            if (!string.IsNullOrEmpty(timerGuid))
            {
                _timers.Remove(timerGuid);
            }
        }

        public float GetTimerSecondsLeft(string timerGuid)
        {
            if (string.IsNullOrEmpty(timerGuid) || !_timers.TryGetValue(timerGuid, out var initialTick))
            {
                return 0f;
            }

            var elapsedTicks = _tickCounterService.CurrentClientTick - initialTick;
            return elapsedTicks * _networkConfig.DeltaTime;
        }
    }
}
