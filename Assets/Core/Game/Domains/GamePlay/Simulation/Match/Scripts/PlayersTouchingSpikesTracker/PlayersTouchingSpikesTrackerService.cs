using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker
{
    public class PlayersTouchingSpikesTrackerService : IPlayersTouchingSpikesTrackerService
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly CapacityDict<ushort, PlayerTouchingSpikesData> _playersTouchingSpikes;
        private readonly ConcurrentPool<PlayerTouchingSpikesData> _playerDataPool;
        private readonly List<PlayerTouchingSpikeToDamageData> _cachedPlayersToDamage;

        public PlayersTouchingSpikesTrackerService(ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig)
        {
            _gamePlayConfigService = gamePlayConfigService;
            var maxPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            _playersTouchingSpikes = new CapacityDict<ushort, PlayerTouchingSpikesData>(maxPlayers);
            _playerDataPool = new ConcurrentPool<PlayerTouchingSpikesData>(() => new PlayerTouchingSpikesData(networkConfig.MaxCap.ConcurrentSpikeCollidingWithPlayer), maxPlayers);
            _cachedPlayersToDamage = new List<PlayerTouchingSpikeToDamageData>(maxPlayers);
        }

        public void OnPlayerBeginTouchSpike(ushort playerId, ushort spikeId)
        {
            if (!_playersTouchingSpikes.ContainsKey(playerId))
            {
                _playersTouchingSpikes.Add(playerId, _playerDataPool.Get());
            }

            _playersTouchingSpikes[playerId].OnBeginTouchSpike(spikeId);
        }

        public void OnPlayerEndTouchSpike(ushort playerId, ushort spikeId)
        {
            if (!_playersTouchingSpikes.TryGetValue(playerId, out var playerData))
            {
                LogService.LogError($"Player {playerId} stopped touching spike {spikeId} but does not exist in touching spikes tracker");
                return;
            }

            playerData.OnEndTouchSpike(spikeId);

            if (playerData.TouchingSpikesCount == 0)
            {
                playerData.Reset();
                _playerDataPool.Return(playerData);
                _playersTouchingSpikes.Remove(playerId);
            }
        }

        public void StepTimePassedSinceLastDamageTaken(FixedUnorderedList<ushort> playerIdsNotToIncrementTimer, float deltaTime)
        {
            foreach (var playerId in _playersTouchingSpikes.Keys)
            {
                if (playerIdsNotToIncrementTimer.Contains(playerId))
                {
                    continue;
                }

                _playersTouchingSpikes[playerId].TimePassSinceLastDamageTaken += deltaTime;
            }
        }

        public List<PlayerTouchingSpikeToDamageData> GetPlayersToDamage()
        {
            _cachedPlayersToDamage.Clear();
            if (_playersTouchingSpikes.IsNullOrEmpty())
            {
                return _cachedPlayersToDamage;
            }

            var damageIntervalInSeconds = _gamePlayConfigService.GamePlayConfig.EnvironmentSpikes.DamageIntervalInSeconds;
            foreach (var playerId in _playersTouchingSpikes.Keys)
            {
                var playerData = _playersTouchingSpikes[playerId];
                var didPassDamageInterval = playerData.TimePassSinceLastDamageTaken >= damageIntervalInSeconds;
                if (didPassDamageInterval)
                {
                    _cachedPlayersToDamage.Add(new PlayerTouchingSpikeToDamageData(playerId, playerData.GetAnyTouchedSpikeId()));
                }
            }

            return _cachedPlayersToDamage;
        }

        public void TryResetPlayerTimePassedSinceLastDamageTaken(ushort playerId)
        {
            if (_playersTouchingSpikes.TryGetValue(playerId, out var playerData))
            {
                playerData.TimePassSinceLastDamageTaken = 0;
            }
        }

        public void ClearAllData()
        {
            foreach (var kvp in _playersTouchingSpikes)
            {
                kvp.Value.Reset();
                _playerDataPool.Return(kvp.Value);
            }

            _playersTouchingSpikes.Clear();
        }

        private class PlayerTouchingSpikesData
        {
            private readonly CapacityDict<ushort, int> _contactsCountPerSpikeId;

            public float TimePassSinceLastDamageTaken;

            public int TouchingSpikesCount => _contactsCountPerSpikeId.Count;

            public PlayerTouchingSpikesData(int maxSpikes)
            {
                _contactsCountPerSpikeId = new CapacityDict<ushort, int>(maxSpikes);
            }

            public void OnBeginTouchSpike(ushort spikeId)
            {
                _contactsCountPerSpikeId.TryGetValue(spikeId, out var contactsCount);
                _contactsCountPerSpikeId[spikeId] = contactsCount + 1;
            }

            public void OnEndTouchSpike(ushort spikeId)
            {
                if (!_contactsCountPerSpikeId.TryGetValue(spikeId, out var contactsCount))
                {
                    LogService.LogError($"Spike {spikeId} contact ended but was not tracked for this player");
                    return;
                }

                var contactsCountLeft = contactsCount - 1;
                if (contactsCountLeft <= 0)
                {
                    _contactsCountPerSpikeId.Remove(spikeId);
                    return;
                }

                _contactsCountPerSpikeId[spikeId] = contactsCountLeft;
            }

            public ushort GetAnyTouchedSpikeId()
            {
                foreach (var kvp in _contactsCountPerSpikeId)
                {
                    return kvp.Key;
                }

                return default;
            }

            public void Reset()
            {
                _contactsCountPerSpikeId.Clear();
                TimePassSinceLastDamageTaken = 0;
            }
        }
    }
}
