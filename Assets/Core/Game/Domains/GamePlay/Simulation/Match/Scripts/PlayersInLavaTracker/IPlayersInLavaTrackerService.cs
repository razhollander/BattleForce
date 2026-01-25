using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker
{
    public interface IPlayersInLavaTrackerService
    {
        void OnPlayerEnterLava(ushort playerId);
        void OnPlayerExitLava(ushort playerId);
        void StepTimePassedSinceLastDamageTaken(float deltaTime);
        List<ushort> GetPlayerIdsToDamage();
        void ResetPlayerTimePassedSinceLastDamageTaken(ushort playerId);
    }
}