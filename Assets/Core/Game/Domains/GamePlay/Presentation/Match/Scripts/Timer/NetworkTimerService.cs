using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public class NetworkTimerService : INetworkTimerService
    {
        private readonly NetworkConfig _networkConfig;
        private readonly Dictionary<string, int> _timers = new Dictionary<string, int>();

        public NetworkTimerService(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
        }

        public string StartTimer(int initialServerTick)
        {
            var guid = Guid.NewGuid().ToString();
            _timers.Add(guid, initialServerTick);
            return guid;
        }

        public void CancelTimer(string timerGuid)
        {
            if (!_timers.ContainsKey(timerGuid))
            {
                LogService.LogError("No timer found with guid: " + timerGuid);
                return;
            }
            
            _timers.Remove(timerGuid);
        }

        public float GetTimerSecondsPassed(string timerGuid, int currentServerTick)
        {
            if (!_timers.TryGetValue(timerGuid, out var initialServerTick))
            {
                LogService.LogError("No timer found with guid: " + timerGuid);
                return 0f;
            }

            var elapsedTicks =  currentServerTick - initialServerTick;
            return elapsedTicks * _networkConfig.DeltaTime;
        }
    }
}
