using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker
{
    public class PlayersInLavaTrackerService : IPlayersInLavaTrackerService
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly CapacityDict<ushort, PlayerInLavaData> _playersInLava;
        private readonly ConcurrentPool<PlayerInLavaData> _playerInLavaDataPool;
        private readonly List<ushort> _cachedPlayersToDamage;

        public PlayersInLavaTrackerService(ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig)
        {
            _gamePlayConfigService = gamePlayConfigService;
            _playersInLava = new CapacityDict<ushort, PlayerInLavaData>(networkConfig.MaxCap.ConcurrentPlayers);
            _cachedPlayersToDamage = new List<ushort>(networkConfig.MaxCap.ConcurrentPlayers);
            _playerInLavaDataPool = new ConcurrentPool<PlayerInLavaData>(() => new PlayerInLavaData(), networkConfig.MaxCap.ConcurrentPlayers);
        }
        
        public void OnPlayerEnterLava(ushort playerId)
        {
            var didStartBeingExposedToLava = !_playersInLava.ContainsKey(playerId);
            if (didStartBeingExposedToLava)
            {
                var playerInLavaData = _playerInLavaDataPool.Get();
                _playersInLava.Add(playerId, playerInLavaData);
            }

            _playersInLava[playerId].LavaAmountPlayerIsIn++;
        }

        public void OnPlayerExitLava(ushort playerId)
        {
            if (_playersInLava.ContainsKey(playerId))
            {
                var playerInLavaData = _playersInLava[playerId];
                var lavaAmountPlayerIsIn = --playerInLavaData.LavaAmountPlayerIsIn;
                if (lavaAmountPlayerIsIn <= 0)
                {
                    playerInLavaData.Reset();
                    _playerInLavaDataPool.Return(playerInLavaData);
                    _playersInLava.Remove(playerId);
                }
            }
            else
            {
                LogService.LogError($"Player {playerId} exit lava but does not exist in lava");
            }
        }

        public bool IsPlayerInLava(ushort playerId)
        {
            return _playersInLava.ContainsKey(playerId);
        }

        public void StepTimePassedSinceLastDamageTaken(FixedUnorderedList<ushort> playerIdsNotToIncrementTimerInLava, float deltaTime)
        {
            foreach (var playerId in _playersInLava.Keys)
            {
                if (playerIdsNotToIncrementTimerInLava.Contains(playerId))
                {
                    continue;
                }
                
                _playersInLava[playerId].TimePassSinceLastDamageTaken += deltaTime;
            }
        }

        public List<ushort> GetPlayerIdsToDamage()
        {
            _cachedPlayersToDamage.Clear();
            if (_playersInLava.Count == 0) return _cachedPlayersToDamage;

            foreach (var playerId in _playersInLava.Keys)
            {
                var didPassDamageInterval = _playersInLava[playerId].TimePassSinceLastDamageTaken >= _gamePlayConfigService.GamePlayConfig.Lava.DamageIntervalInSeconds;
                if (didPassDamageInterval)
                {
                    _cachedPlayersToDamage.Add(playerId);
                }
            }

            return _cachedPlayersToDamage;
        }

        public void ClearAllData()
        {
            foreach (var kvp in _playersInLava)
            {
                kvp.Value.Reset();
                _playerInLavaDataPool.Return(kvp.Value);
            }
            
            _playersInLava.Clear();
        }
        
        public void TryResetPlayerTimePassedSinceLastDamageTaken(ushort playerId)
        {
            if (_playersInLava.TryGetValue(playerId, out var playerInLava))
            {
                playerInLava.TimePassSinceLastDamageTaken = 0;
            }
        }
        
        private class PlayerInLavaData
        {
            public float TimePassSinceLastDamageTaken;
            public int LavaAmountPlayerIsIn;

            public void Reset()
            {
                TimePassSinceLastDamageTaken = 0;
                LavaAmountPlayerIsIn = 0;
            }
        }
    }
}
