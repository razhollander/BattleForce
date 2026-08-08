using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker
{
    public interface IPlayersTouchingSpikesTrackerService
    {
        void OnPlayerBeginTouchSpike(ushort playerId, ushort spikeId);
        void OnPlayerEndTouchSpike(ushort playerId, ushort spikeId);
        void StepTimePassedSinceLastDamageTaken(FixedUnorderedList<ushort> playerIdsNotToIncrementTimer, float deltaTime);
        List<PlayerTouchingSpikeToDamage> GetPlayersToDamage();
        void TryResetPlayerTimePassedSinceLastDamageTaken(ushort playerId);
        void ClearAllData();
    }

    public readonly struct PlayerTouchingSpikeToDamage
    {
        public readonly ushort PlayerId;
        public readonly ushort SpikeId;

        public PlayerTouchingSpikeToDamage(ushort playerId, ushort spikeId)
        {
            PlayerId = playerId;
            SpikeId = spikeId;
        }
    }
}
