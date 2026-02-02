using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public class StageDataService : IStageDataService
    {
        private readonly IMatchDataService _matchDataService;
        public Queue<ushort> OrderedTeamIdsLost { get; private set; }
        public ushort WinnerTeamId;
        public bool IsMatchEnded { get; set; }

        public void ClearData()
        {
            OrderedTeamIdsLost.Clear();
            WinnerTeamId = 0;
        }
        
        public void AddLosingTeam(ushort teamId)
        {
            OrderedTeamIdsLost.Enqueue(teamId);
        }
        
        public void AddWinnerTeam(ushort teamId)
        {
            WinnerTeamId = teamId;
        }
        
        public StageDataService(IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchDataService = matchDataService;
            OrderedTeamIdsLost = new Queue<ushort>(sharedGamePlayConfig.MaxTeamsAmount-1);
        }

        public Dictionary<ushort, int> GetJemsCollectedPerTeam()
        {
            var jemsCollectedPerTeam = new Dictionary<ushort, int>();
            var amountOfJemsForTeam = 1;

            while (OrderedTeamIdsLost.TryDequeue(out var teamId))
            {
                jemsCollectedPerTeam.Add(teamId, amountOfJemsForTeam++);
            }

            jemsCollectedPerTeam.Add(WinnerTeamId, amountOfJemsForTeam);
            return jemsCollectedPerTeam;
        }
    }
}