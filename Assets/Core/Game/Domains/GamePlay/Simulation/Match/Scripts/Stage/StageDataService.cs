using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public class StageDataService : IStageDataService
    {
        private readonly IMatchDataService _matchDataService;
        public ushort WinnerTeamId;
        public HashSet<ushort> LosingTeamIds { get; private set; }
        public Dictionary<ushort, int> GemsCollectedPerTeam { get; private set; }
        public bool IsStageEnded { get; set; }
        public bool IsInPreparationPhase { get; set; }
        public int StartPhaseInitialTick { get; set; }
        public float StageRestartTimer { get; set; }

        public StageDataService(IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchDataService = matchDataService;
            LosingTeamIds = new HashSet<ushort>(sharedGamePlayConfig.MaxTeamsAmount);
        }

        public void AddLosingTeam(ushort teamId)
        {
            LosingTeamIds.Add(teamId);
        }

        public void InitEntryPoint()
        {
            var teamIds = _matchDataService.TeamIds;
            GemsCollectedPerTeam = new Dictionary<ushort, int>(teamIds.Count);

            foreach (ushort teamId in teamIds)
            {
                GemsCollectedPerTeam.Add(teamId, 0);
            }
        }

        public void ClearData()
        {
            StartPhaseInitialTick = 0;
            WinnerTeamId = 0;
            LosingTeamIds.Clear();

            foreach (var teamId in _matchDataService.TeamIds)
            {
                GemsCollectedPerTeam[teamId] = 0;
            }
        }

        public void AddGemsForTeam(ushort teamAlive, int gemsDelta)
        {
            GemsCollectedPerTeam[teamAlive] += gemsDelta;
        }

        public void AddWinnerTeam(ushort teamId)
        {
            WinnerTeamId = teamId;
        }
    }
}