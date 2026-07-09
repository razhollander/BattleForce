using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker
{
    public interface IPlayersInLavaTrackerService
    {
        void OnPlayerEnterLava(ushort playerId);
        void OnPlayerExitLava(ushort playerId);
        void StepTimePassedSinceLastDamageTaken(FixedUnorderedList<ushort> playerIdsNotToIncrementTimerInLava, float deltaTime);
        List<ushort> GetPlayerIdsToDamage();
        void ResetPlayerTimePassedSinceLastDamageTaken(ushort playerId);
        void ClearAllData();
    }
}