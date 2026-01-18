using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public interface IPlayersInLavaTrackerService
    {
        void OnPlayerEnterLava(ushort playerId);
        void OnPlayerExitLava(ushort playerId);
        List<ushort> StepAndGetPlayerIdsToDamage(float deltaTime);
    }

    public class PlayersInLavaTrackerService : IPlayersInLavaTrackerService
    {
        private readonly SimulationGamePlayConfig _gamePlayerConfig;
        private readonly CapacityDict<ushort, PlayerInLavaData> _playersInLava;
        private readonly ConcurrentPool<PlayerInLavaData> _playerInLavaDataPool;
        private readonly List<ushort> _playersToDamage;

        public PlayersInLavaTrackerService(SimulationGamePlayConfig gamePlayerConfig, NetworkConfig networkConfig)
        {
            _gamePlayerConfig = gamePlayerConfig;
            _playersInLava = new CapacityDict<ushort, PlayerInLavaData>(networkConfig.MaxCap.ConcurrentPlayers);
            _playersToDamage = new List<ushort>(networkConfig.MaxCap.ConcurrentPlayers);
            _playerInLavaDataPool = new ConcurrentPool<PlayerInLavaData>(() => new PlayerInLavaData(), networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void OnPlayerEnterLava(ushort playerId)
        {
            if (!_playersInLava.ContainsKey(playerId))
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
                var lavaAmountPlayerIsIn = --_playersInLava[playerId].LavaAmountPlayerIsIn;
                if (lavaAmountPlayerIsIn <= 0)
                {
                    _playersInLava.Remove(playerId);
                }
            }
            else
            {
                LogService.LogError($"Player {playerId} exit lava but does not exist in lava");
            }
        }
        
        public List<ushort> StepAndGetPlayerIdsToDamage(float deltaTime)
        {
            _playersToDamage.Clear();
            if (_playersInLava.Count == 0) return _playersToDamage;

            foreach (var playerId in _playersInLava.Keys)
            {
                _playersInLava[playerId].TimePassSinceLastDamageTaken += deltaTime;
                var didPassDamageInterval = _playersInLava[playerId].TimePassSinceLastDamageTaken >= _gamePlayerConfig.Lava.DamageIntervalInSeconds;
                if (didPassDamageInterval)
                {
                    _playersToDamage.Add(playerId);
                    _playersInLava[playerId].TimePassSinceLastDamageTaken = 0;
                }
            }

            return _playersToDamage;
        }
        
        private class PlayerInLavaData
        {
            public float TimePassSinceLastDamageTaken;
            public int LavaAmountPlayerIsIn;
        }
    }
}
