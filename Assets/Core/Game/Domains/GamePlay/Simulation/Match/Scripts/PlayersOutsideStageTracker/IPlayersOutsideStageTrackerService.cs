using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker
{
    public interface IPlayersOutsideStageTrackerService
    {
        void OnPlayerEnterStageBoundary(ushort playerId);
        void OnPlayerExitStageBoundary(ushort playerId);
        bool IsPlayerOutside(ushort playerId);
        void ClearAllData();
    }
}
