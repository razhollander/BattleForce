using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public class StageDataService : IStageDataService
    {
        private readonly IMatchDataService _matchDataService;
        public HashSet<ushort> LosingTeamIds { get; private set; }
        public Dictionary<ushort, int> GemsCollectedPerTeam { get; private set; }
        public bool IsStageEnded { get; set; }
        public float StageRestartTimer { get; set; }
        public int AmountOfStagesEntered { get; private set; }
        public bool IsWhacAMoleStage => _matchDataService.SimulationState.StageType == StageType.WhacAMole;
        public bool IsBonusStage => _matchDataService.SimulationState.StageType.IsBonusStage();

        public StageDataService(IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchDataService = matchDataService;
            LosingTeamIds = new HashSet<ushort>(sharedGamePlayConfig.MaxTeamsAmount);
        }

        public void AddLosingTeam(ushort teamId)
        {
            LosingTeamIds.Add(teamId);
        }

        public void IncrementStagesEnteredAmount()
        {
            AmountOfStagesEntered++;
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
            LosingTeamIds.Clear();
            IsStageEnded = false;
            StageRestartTimer = -1;
            
            foreach (var teamId in _matchDataService.TeamIds)
            {
                GemsCollectedPerTeam[teamId] = 0;
            }
        }

        public void AddGemsForTeam(ushort teamAlive, int gemsDelta)
        {
            GemsCollectedPerTeam[teamAlive] += gemsDelta;
        }
    }
}