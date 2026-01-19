using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public interface IPlayersInLavaTrackerService
    {
        void OnPlayerEnterLava(ushort playerId);
        void OnPlayerExitLava(ushort playerId);
        List<ushort> StepAndGetPlayerIdsToDamage(float deltaTime);
    }
}