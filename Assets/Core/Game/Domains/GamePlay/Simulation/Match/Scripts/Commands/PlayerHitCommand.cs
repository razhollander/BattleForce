using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class PlayerHitCommand : BaseCommand, ICommandVoid
    {
        private const ushort DEAD_HEALTH_AMOUNT = 0;
        
        private ushort _hitDamage;
        private ushort _playerId;
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private ICommandFactory _commandFactory;
        private IStageDataService _stageDataService;
        private SimulationGamePlayConfig _gamePlayConfig;
        private int _processedTick;

        public PlayerHitCommand SetHitDamage(ushort hitDamage)
        {
            _hitDamage = hitDamage;
            return this;
        }

        public PlayerHitCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public PlayerHitCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService =_diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            var sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(_playerId);

            if (!playerState.Spaceship.IsAlive)
            {
                return;
            }
            
            var newHealth = (ushort)Math.Max(DEAD_HEALTH_AMOUNT, playerState.Spaceship.Health.CurrentHealth - _hitDamage);
            playerState.Spaceship.Health.CurrentHealth = newHealth;
            var isPlayerAlive = newHealth > DEAD_HEALTH_AMOUNT;

          
            LogService.LogTopic($"Player Hit! Id {_playerId} hit with damage {_hitDamage}, new health: {newHealth}, is alive: {isPlayerAlive}", LogTopicType.ServerNetwork);
            _netEventsDataService.AddPlayerTakeDamageNetEvent(_processedTick, _playerId, newHealth, _hitDamage, isPlayerAlive);

            if (!isPlayerAlive)
            {
                playerState.Spaceship.IsAlive = false;

                var shootState = playerState.Spaceship.Shoot;
                shootState.MaxCooldown *= _gamePlayConfig.ShootCooldownMultiplierWhenDead;
                playerState.Spaceship.Shoot = shootState;

                _netEventsDataService.AddPlayerDiedNetEvent(_processedTick, _playerId, shootState.MaxCooldown);

                if (!_stageDataService.IsMatchEnded)
                {
                    MarkTeamIfLost(playerState.TeamId);
                    TryInvokeMatchEnded();
                }
            }
        }

        private void MarkTeamIfLost(ushort teamId)
        {
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                var isPlayerAliveInTeam = player.TeamId == teamId && player.Spaceship.IsAlive;
                if (isPlayerAliveInTeam)
                {
                    return;
                }
            }
            
            _stageDataService.AddLosingTeam(teamId);
        }
        
        private void TryInvokeMatchEnded()
        {
            if (!TryFindWinningTeam(out ushort winningTeamId))
            {
                return;
            }

            _commandFactory.CreateCommandVoid<StageEndedCommand>()
                .SetWinningTeamId(winningTeamId)
                .SetProcessedTick(_processedTick)
                .Execute();
        }

        private bool TryFindWinningTeam(out ushort winningTeamId)
        {
            var didFindAliveTeam = false;
            winningTeamId = 0;
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!player.Spaceship.IsAlive)
                {
                    continue;
                }

                if (didFindAliveTeam && player.TeamId != winningTeamId)
                {
                    return false;
                }
                    
                winningTeamId = player.TeamId;
                didFindAliveTeam = true;
            }

            return true;
        }
    }
}
