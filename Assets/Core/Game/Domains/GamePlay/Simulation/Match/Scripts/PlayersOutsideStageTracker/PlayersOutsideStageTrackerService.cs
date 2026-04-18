using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker
{
    public class PlayersOutsideStageTrackerService : IPlayersOutsideStageTrackerService
    {
        private readonly CapacityDict<ushort, PlayerInStageData> _playersInStage;
        private readonly ConcurrentPool<PlayerInStageData> _playerInStageDataPool;

        public PlayersOutsideStageTrackerService(NetworkConfig networkConfig)
        {
            _playersInStage = new CapacityDict<ushort, PlayerInStageData>(networkConfig.MaxCap.ConcurrentPlayers);
            _playerInStageDataPool = new ConcurrentPool<PlayerInStageData>(() => new PlayerInStageData(), networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void OnPlayerEnterStageBoundary(ushort playerId)
        {
            if (!_playersInStage.ContainsKey(playerId))
            {
                var data = _playerInStageDataPool.Get();
                _playersInStage.Add(playerId, data);
            }

            _playersInStage[playerId].BoundariesPlayerIsIn++;
        }

        public void OnPlayerExitStageBoundary(ushort playerId)
        {
            if (_playersInStage.ContainsKey(playerId))
            {
                var data = _playersInStage[playerId];
                var boundariesPlayerIsIn = --data.BoundariesPlayerIsIn;
                if (boundariesPlayerIsIn <= 0)
                {
                    data.Reset();
                    _playerInStageDataPool.Return(data);
                    _playersInStage.Remove(playerId);
                }
            }
            else
            {
                LogService.LogError($"Player {playerId} exit stage boundary but does not exist in stage tracker");
            }
        }

        public bool IsPlayerOutside(ushort playerId)
        {
            if (!_playersInStage.ContainsKey(playerId))
            {
                return true;
            }

            return _playersInStage[playerId].BoundariesPlayerIsIn <= 0;
        }

        public void ClearAllData()
        {
            foreach (var kvp in _playersInStage)
            {
                kvp.Value.Reset();
                _playerInStageDataPool.Return(kvp.Value);
            }

            _playersInStage.Clear();
        }

        private class PlayerInStageData
        {
            public int BoundariesPlayerIsIn;

            public void Reset()
            {
                BoundariesPlayerIsIn = 0;
            }
        }
    }
}
