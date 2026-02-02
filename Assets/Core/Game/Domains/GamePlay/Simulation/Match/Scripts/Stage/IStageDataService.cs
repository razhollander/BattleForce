using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public interface IStageDataService
    {
        Queue<ushort> OrderedTeamIdsLost { get; }
        void AddLosingTeam(ushort teamId);
        void AddWinnerTeam(ushort teamId);
        Dictionary<ushort, int> GetJemsCollectedPerTeam();
        bool IsMatchEnded { get; set; }
    }
}