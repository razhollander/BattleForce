using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall
{
    public class LockOnWallTimerService : ILockOnWallTimerService
    {
        private readonly IMatchMakingDataService _matchMakingDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly Dictionary<ushort, float> _timerPerPlayer;

        public LockOnWallTimerService(IMatchMakingDataService matchMakingDataService, SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig)
        {
            _matchMakingDataService = matchMakingDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _timerPerPlayer = new Dictionary<ushort, float>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void StepTimers(float deltaTime)
        {
            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())
            {
                var playerId = playerState.Id;

                if (!playerState.Spaceship.IsLockingOnWall)
                {
                    _timerPerPlayer.Remove(playerId);
                    continue;
                }

                _timerPerPlayer[playerId] = _timerPerPlayer.TryGetValue(playerId, out var timer) ? timer + deltaTime : deltaTime;
            }
        }

        public bool IsWallShootableByPlayer(ushort playerId)
        {
            return _timerPerPlayer.TryGetValue(playerId, out var timer) && timer >= _sharedGamePlayConfig.LockOnTargetDurationInSeconds;
        }

        public void ResetPlayerTimer(ushort playerId)
        {
            if (_timerPerPlayer.ContainsKey(playerId))
            {
                _timerPerPlayer[playerId] = 0f;
            }
        }
    }
}
