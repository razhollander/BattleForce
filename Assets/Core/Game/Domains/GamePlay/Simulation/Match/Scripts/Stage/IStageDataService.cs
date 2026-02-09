using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public interface IStageDataService
    {
        HashSet<ushort> LosingTeamIds { get; }
        void AddLosingTeam(ushort teamId);
        void AddWinnerTeam(ushort teamId);
        bool IsStageEnded { get; set; }
        float StageRestartTimer { get; set; }
        void ClearData();
        Dictionary<ushort, int> GemsCollectedPerTeam { get; }
        void AddGemsForTeam(ushort teamAlive, ushort gemsDelta);
        void InitEntryPoint();
    }
}