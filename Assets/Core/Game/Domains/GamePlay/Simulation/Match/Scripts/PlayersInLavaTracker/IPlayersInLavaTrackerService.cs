using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker
{
    public interface IPlayersInLavaTrackerService
    {
        /// <summary>Returns true when the player just became exposed to lava (was not in any lava before).</summary>
        bool OnPlayerEnterLava(ushort playerId);
        /// <summary>Returns true when the player just stopped being exposed to lava (left the last lava it was in).</summary>
        bool OnPlayerExitLava(ushort playerId);
        /// <summary>Returns true while the player is physically standing in at least one lava body.</summary>
        bool IsPlayerInLava(ushort playerId);
        void StepTimePassedSinceLastDamageTaken(FixedUnorderedList<ushort> playerIdsNotToIncrementTimerInLava, float deltaTime);
        List<ushort> GetPlayerIdsToDamage();
        void TryResetPlayerTimePassedSinceLastDamageTaken(ushort playerId);
        void ClearAllData();
    }
}