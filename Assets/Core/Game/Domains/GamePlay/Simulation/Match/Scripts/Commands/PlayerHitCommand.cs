using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
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
        }

        public void Execute()
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(_playerId);
            var newHealth = (ushort)Math.Max(DEAD_HEALTH_AMOUNT, playerState.Spaceship.Health.CurrentHealth - _hitDamage);
            playerState.Spaceship.Health.CurrentHealth = newHealth;
            var isPlayerAlive = newHealth > DEAD_HEALTH_AMOUNT;

            if (!isPlayerAlive)
            {
                playerState.IsAlive = false;
            }

            LogService.LogTopic($"Player Hit! Id {_playerId} hit with damage {_hitDamage}, new health: {newHealth}, is alive: {isPlayerAlive}", LogTopicType.ServerNetwork);
            _netEventsDataService.AddPlayerTakeDamageNetEvent(_processedTick, _playerId, newHealth, _hitDamage, isPlayerAlive);

            if (!isPlayerAlive)
            {
                CheckMatchEnded();
            }
        }

        private void CheckMatchEnded()
        {
            if (_matchDataService.IsMatchEnded)
            {
                return;
            }

            var aliveTeams = new HashSet<ushort>();
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (player.IsAlive)
                {
                    aliveTeams.Add(player.TeamId);
                }
            }

            if (aliveTeams.Count <= 1)
            {
                ushort winningTeamId = 0;
                foreach (var teamId in aliveTeams)
                {
                    winningTeamId = teamId;
                    break;
                }

                if (aliveTeams.Count == 1)
                {
                    _commandFactory.CreateCommandVoid<StageEndedCommand>()
                        .SetWinningTeamId(winningTeamId)
                        .SetProcessedTick(_processedTick)
                        .Execute();
                }
                else if (aliveTeams.Count == 0)
                {
                    LogService.LogWarning("All players died! No winner?");
                    _commandFactory.CreateCommandVoid<StageEndedCommand>()
                        .SetWinningTeamId(0)
                        .SetProcessedTick(_processedTick)
                        .Execute();
                }
            }
        }
    }
}
