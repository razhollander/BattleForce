using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker
{
    public interface IPlayersTouchingSpikesTrackerService
    {
        void OnPlayerBeginTouchSpike(ushort playerId, ushort spikeId);
        void OnPlayerEndTouchSpike(ushort playerId, ushort spikeId);
        void StepTimePassedSinceLastDamageTaken(FixedUnorderedList<ushort> playerIdsNotToIncrementTimer, float deltaTime);
        List<PlayerTouchingSpikeToDamageData> GetPlayersToDamage();
        void TryResetPlayerTimePassedSinceLastDamageTaken(ushort playerId);
        void ClearAllData();
    }
}
