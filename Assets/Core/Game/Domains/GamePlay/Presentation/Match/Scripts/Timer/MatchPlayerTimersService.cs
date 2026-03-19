using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public class MatchPlayerTimersService : IMatchPlayerTimersService
    {
        private readonly INetworkTimerService _networkTimerService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly Dictionary<ushort, MatchPlayerTimers> _playerTimers = new Dictionary<ushort, MatchPlayerTimers>();

        public MatchPlayerTimersService(INetworkTimerService networkTimerService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _networkTimerService = networkTimerService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        private MatchPlayerTimers GetOrCreatePlayerTimers(ushort playerId)
        {
            if (!_playerTimers.TryGetValue(playerId, out var timers))
            {
                timers = new MatchPlayerTimers(playerId, _sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
                _playerTimers.Add(playerId, timers);
            }
            return timers;
        }

        public void StartPlayerTalentTimer(ushort playerId, int talentIndex, int initialServerTick)
        {
            var timers = GetOrCreatePlayerTimers(playerId);

            // Cancel existing timer if any
            var existingGuid = timers.TalentTimers[talentIndex];
            if (!string.IsNullOrEmpty(existingGuid))
            {
                _networkTimerService.CancelTimer(existingGuid);
            }

            var guid = _networkTimerService.StartTimer(initialServerTick);
            timers.TalentTimers[talentIndex] = guid;
        }

        public float GetPlayerTalentTimer(ushort playerId, int talentIndex, int currentServerTick)
        {
            if (!_playerTimers.TryGetValue(playerId, out var timers))
            {
                return 0f;
            }

            var guid = timers.TalentTimers[talentIndex];
            if (string.IsNullOrEmpty(guid))
            {
                return 0f;
            }

            var elapsedTime = _networkTimerService.GetTimerSecondsPassed(guid, currentServerTick);
            // var maxCooldown = _matchDataService.GetPlayer(playerId).Spaceship.TalentsState.Talents[talentIndex].MaxCooldown;
            // var secondsLeft = maxCooldown - elapsedTime;
            //
            // if (secondsLeft <= 0)
            // {
            //     _networkTimerService.CancelTimer(guid);
            //     timers.TalentTimers[talentIndex] = null;
            //     return 0f;
            // }

            return elapsedTime;
        }
    }
}
