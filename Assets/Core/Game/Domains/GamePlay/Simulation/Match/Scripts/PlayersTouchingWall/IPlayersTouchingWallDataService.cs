using System.Collections.Generic;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall
{
    public interface IPlayersTouchingWallDataService
    {
        void OnPlayerBeginTouchWall(ushort playerId, ushort wallId, Vector2 wallNormal, int tick);
        void OnPlayerEndTouchWall(ushort playerId, ushort wallId);

        /// <summary>
        /// Returns a cached list of every (player, wall normal) pair where the player has been touching that wall
        /// for at least <paramref name="minTicksTouching"/> ticks (the players referred to as 'PlayersStickToWall').
        /// </summary>
        List<PlayerStickToWallData> GetPlayersStickToWall(int currentTick, int minTicksTouching);

        void ClearAllData();
    }
}
