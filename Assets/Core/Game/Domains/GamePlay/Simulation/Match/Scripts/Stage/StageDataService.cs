using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public class StageDataService : IStageDataService
    {
        private readonly IMatchDataService _matchDataService;
        public ushort WinnerTeamId;
        public HashSet<ushort> LosingTeamIds { get; private set; }
        public Dictionary<ushort, int> GemsCollectedPerTeam { get; private set; }
        public bool IsStageEnded { get; set; }
        public float StageRestartTimer { get; set; }

        public void AddLosingTeam(ushort teamId)
        {
            LosingTeamIds.Add(teamId);
        }

        public void ClearData()
        {
            WinnerTeamId = 0;
            LosingTeamIds.Clear();
            GemsCollectedPerTeam.Clear();
        }

        public void AddGemsForTeam(ushort teamAlive, ushort gemsCollectedForTeamAlive)
        {
            GemsCollectedPerTeam.Add(teamAlive, gemsCollectedForTeamAlive);
        }

        public void AddWinnerTeam(ushort teamId)
        {
            WinnerTeamId = teamId;
        }
        
        public StageDataService(IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchDataService = matchDataService;
            LosingTeamIds = new HashSet<ushort>(sharedGamePlayConfig.MaxTeamsAmount);
            GemsCollectedPerTeam = new Dictionary<ushort, int>(sharedGamePlayConfig.MaxTeamsAmount);
        }
    }
}