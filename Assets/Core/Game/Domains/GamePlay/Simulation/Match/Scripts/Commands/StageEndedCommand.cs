using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StageEndedCommand : BaseCommand, ICommandVoid
    {
        private ushort _winningTeamId;
        private int _processedTick;

        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private SharedGamePlayConfig _sharedGamePlayConfig;

        public StageEndedCommand SetWinningTeamId(ushort winningTeamId)
        {
            _winningTeamId = winningTeamId;
            return this;
        }

        public StageEndedCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        {
            LogService.LogTopic($"Match Ended! Winning Team: {_winningTeamId}", LogTopicType.ServerNetwork);

            var players = _matchDataService.SimulationState.Players;
            var jemsPerTeam = _matchDataService.SimulationState.JemsPerTeamId;
            var jemsWonPerTeam = new Dictionary<ushort, int>();

            // Calculate jems for all teams present in the match
            // Iterate over players to find all teams
            var teams = new HashSet<ushort>();
            foreach(var player in players.AsSpan())
            {
                teams.Add(player.TeamId);
            }

            foreach(var teamId in teams)
            {
                int jemsWon = (teamId == _winningTeamId) ? _sharedGamePlayConfig.WinJemsAmount : _sharedGamePlayConfig.WinJemsAmount - 1;
                jemsWonPerTeam[teamId] = jemsWon;

                if(!jemsPerTeam.ContainsKey(teamId))
                {
                    jemsPerTeam[teamId] = 0;
                }
                jemsPerTeam[teamId] += jemsWon;
            }

            _netEventsDataService.AddStageEndNetEvent(_processedTick, _winningTeamId, jemsWonPerTeam, jemsPerTeam);
            _matchDataService.IsMatchEnded = true;
        }
    }
}
