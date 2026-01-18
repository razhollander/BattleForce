using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public interface ILavaManager
    {
        void InitEntryPoint();
        void OnPlayerEnterLava(ushort playerId, int currentTick);
        void OnPlayerExitLava(ushort playerId);
        void SetProcessedTick(int currentTick);
    }

    public class LavaManager : ILavaManager, ITickable
    {
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _config;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;

        private readonly Dictionary<ushort, float> _playersInLava = new Dictionary<ushort, float>();
        private readonly Dictionary<ushort, int> _playerOverlapCount = new Dictionary<ushort, int>();
        private readonly List<ushort> _playersToRemove = new List<ushort>();
        private readonly List<ushort> _playersToDamage = new List<ushort>();

        private int _currentTick;

        private PlayerHitCommand _playerHitCommand;

        public LavaManager(IMatchDataService matchDataService,
                           SimulationGamePlayConfig config,
                           IUpdateSubscriptionService updateSubscriptionService,
                           ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _config = config;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
        }

        public void InitEntryPoint()
        {
            _playerHitCommand = _commandFactory.CreateCommandVoid<PlayerHitCommand>();
            _updateSubscriptionService.RegisterTickable(this);
        }

        public void SetProcessedTick(int currentTick)
        {
            _currentTick = currentTick;
        }

        public void OnPlayerEnterLava(ushort playerId, int currentTick)
        {
            if (!_playerOverlapCount.ContainsKey(playerId))
            {
                _playerOverlapCount[playerId] = 0;
            }

            _playerOverlapCount[playerId]++;

            if (!_playersInLava.ContainsKey(playerId))
            {
                _playersInLava[playerId] = 0f;
            }
        }

        public void OnPlayerExitLava(ushort playerId)
        {
            if (_playerOverlapCount.ContainsKey(playerId))
            {
                _playerOverlapCount[playerId]--;
                if (_playerOverlapCount[playerId] <= 0)
                {
                    _playerOverlapCount.Remove(playerId);
                    _playersInLava.Remove(playerId);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (_playersInLava.Count == 0) return;

            _playersToRemove.Clear();
            _playersToDamage.Clear();

            foreach (var playerId in _playersInLava.Keys)
            {
                var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
                if (playerState.Id == 0)
                {
                    _playersToRemove.Add(playerId);
                    continue;
                }

                _playersInLava[playerId] += deltaTime;

                if (_playersInLava[playerId] >= _config.Lava.DamageInterval)
                {
                    _playersToDamage.Add(playerId);
                }
            }

            foreach(var id in _playersToDamage)
            {
                 _playersInLava[id] -= _config.Lava.DamageInterval;
                 ApplyDamage(id, _config.Lava.DamageAmount);
            }

            foreach(var id in _playersToRemove)
            {
                _playersInLava.Remove(id);
                _playerOverlapCount.Remove(id);
            }
        }

        private void ApplyDamage(ushort playerId, ushort damage)
        {
            _playerHitCommand
                .SetPlayerId(playerId)
                .SetHitDamage(damage)
                .SetProcessedTick(_currentTick)
                .Execute();
        }
    }
}
