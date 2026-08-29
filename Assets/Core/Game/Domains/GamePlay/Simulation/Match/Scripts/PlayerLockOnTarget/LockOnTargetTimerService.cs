using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public class LockOnTargetTimerService : ILockOnTargetTimerService
    {
        private readonly IMatchDataService _matchDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly Dictionary<ushort, PlayerLockOnTargetTimers> _playerTimers;

        public LockOnTargetTimerService(IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _playerTimers = new Dictionary<ushort, PlayerLockOnTargetTimers>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void AddPlayer(ushort casterId)
        {
            _playerTimers[casterId] = new PlayerLockOnTargetTimers(casterId);
        }

        public void StepTimers(float deltaTime)
        {
            var players = _matchDataService.SimulationState.Players;
            for (int i = 0; i < players.Count; i++)
            {
                var casterState = players[i];
                var casterId = casterState.Id;

                if (!_playerTimers.TryGetValue(casterId, out var playerTimer))
                {
                    continue;
                }

                var targetedIds = casterState.Spaceship.LockOnTargetObjects;
                playerTimer.StepTimers(targetedIds, deltaTime);
            }
        }

        public bool IsTargetShootable(ushort casterId, ushort targetId, LockOnTargetType targetType)
        {
            if (!_playerTimers.TryGetValue(casterId, out var playerTimer))
            {
                return false;
            }

            return playerTimer.IsTargetTimerEnded(new LockOnTargetKey(targetId, targetType), _sharedGamePlayConfig.LockOnTargetDurationInSeconds);
        }

        public void ResetTimer(ushort casterId, ushort targetId, LockOnTargetType targetType)
        {
            if (_playerTimers.TryGetValue(casterId, out var playerTimer))
            {
                playerTimer.ResetTimer(new LockOnTargetKey(targetId, targetType));
            }
        }

        public void ResetAllTimers()
        {
            foreach (var playerTimer in _playerTimers)
            {
                playerTimer.Value.ResetAllTimers();
            }
        }
    }
}