using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public interface IStageDataService
    {
        HashSet<ushort> LosingTeamIds { get; }
        void AddLosingTeam(ushort teamId);
        bool IsStageEnded { get; set; }
        float StageRestartTimer { get; set; }
        bool IsWhacAMoleStage { get; }
        bool IsBonusStage { get; }
        // 1 while the first stage of the match is running. Counts stages, not restarts, so it must survive ClearData -
        // it needs no reset because every match builds its own container and therefore its own StageDataService.
        int AmountOfStagesEntered { get; }
        void IncrementStagesEnteredAmount();
        void ClearData();
        Dictionary<ushort, int> GemsCollectedPerTeam { get; }
        void AddGemsForTeam(ushort teamAlive, int gemsDelta);
        void InitEntryPoint();
    }
}