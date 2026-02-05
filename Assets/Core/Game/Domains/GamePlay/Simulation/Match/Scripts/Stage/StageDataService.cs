using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public class StageDataService : IStageDataService
    {
        private readonly IMatchDataService _matchDataService;
        public Queue<ushort> OrderedTeamIdsLost { get; private set; }
        public Dictionary<ushort, int> GemsPerTeam { get; private set; }
        public ushort WinnerTeamId;
        public bool IsStageEnded { get; set; }
        public float StageRestartTimer { get; set; }

        public void ClearData()
        {
            OrderedTeamIdsLost.Clear();
            GemsPerTeam.Clear();
            WinnerTeamId = 0;
        }

        public void AddGems(ushort teamId, int amount)
        {
            if (GemsPerTeam.ContainsKey(teamId))
            {
                GemsPerTeam[teamId] += amount;
            }
            else
            {
                GemsPerTeam.Add(teamId, amount);
            }
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
            GemsPerTeam = new Dictionary<ushort, int>(sharedGamePlayConfig.MaxTeamsAmount);
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

            // Add gems collected during the match
            foreach (var kvp in GemsPerTeam)
            {
                if (jemsCollectedPerTeam.ContainsKey(kvp.Key))
                {
                    jemsCollectedPerTeam[kvp.Key] += kvp.Value;
                }
                else
                {
                    jemsCollectedPerTeam.Add(kvp.Key, kvp.Value);
                }
            }

            return jemsCollectedPerTeam;
        }
    }
}