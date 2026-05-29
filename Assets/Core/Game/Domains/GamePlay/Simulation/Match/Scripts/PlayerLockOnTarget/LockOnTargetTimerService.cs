using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public class LockOnTargetTimerService : ILockOnTargetTimerService
    {
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;

        // Maps casterId -> targetId -> timer
        private readonly Dictionary<ushort, Dictionary<ushort, float>> _lockOnTimers;

        private readonly List<(ushort CasterId, ushort TargetId)> _cachedPlayersToDamage;

        public LockOnTargetTimerService(IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;

            _lockOnTimers = new Dictionary<ushort, Dictionary<ushort, float>>(networkConfig.MaxCap.ConcurrentPlayers);
            _cachedPlayersToDamage = new List<(ushort CasterId, ushort TargetId)>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void StepTimers(float deltaTime)
        {
            var players = _matchDataService.SimulationState.Players;
            for (int i = 0; i < players.Count; i++)
            {
                var casterState = players[i];
                var casterId = casterState.Id;

                if (!_lockOnTimers.TryGetValue(casterId, out var casterTimers))
                {
                    casterTimers = new Dictionary<ushort, float>();
                    _lockOnTimers[casterId] = casterTimers;
                }

                var targetedIds = casterState.Spaceship.TargetedEnemyIds;

                // Remove targets that are no longer targeted
                var targetsToRemove = new List<ushort>();
                foreach (var targetId in casterTimers.Keys)
                {
                    if (!targetedIds.Contains(targetId))
                    {
                        targetsToRemove.Add(targetId);
                    }
                }
                foreach (var targetId in targetsToRemove)
                {
                    casterTimers.Remove(targetId);
                }

                // Step timers for current targets
                for (int j = 0; j < targetedIds.Count; j++)
                {
                    var targetId = targetedIds[j];
                    if (casterTimers.TryGetValue(targetId, out var timer))
                    {
                        casterTimers[targetId] = timer + deltaTime;
                    }
                    else
                    {
                        casterTimers[targetId] = deltaTime;
                    }
                }
            }
        }

        public List<(ushort CasterId, ushort TargetId)> GetPlayersToDamage()
        {
            _cachedPlayersToDamage.Clear();
            var limit = _gamePlayConfig.LockOnTargetHitDurationInSeconds;

            foreach (var casterKvp in _lockOnTimers)
            {
                var casterId = casterKvp.Key;
                foreach (var targetKvp in casterKvp.Value)
                {
                    if (targetKvp.Value >= limit)
                    {
                        _cachedPlayersToDamage.Add((casterId, targetKvp.Key));
                    }
                }
            }

            return _cachedPlayersToDamage;
        }

        public void ResetTimer(ushort casterId, ushort targetId)
        {
            if (_lockOnTimers.TryGetValue(casterId, out var casterTimers))
            {
                if (casterTimers.ContainsKey(targetId))
                {
                    casterTimers[targetId] = 0f;
                }
            }
        }
    }
}
