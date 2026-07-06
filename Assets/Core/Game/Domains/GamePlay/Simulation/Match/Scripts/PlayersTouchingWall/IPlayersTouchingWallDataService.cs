using System.Collections.Generic;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall
{
    public interface IPlayersTouchingWallDataService
    {
        void OnPlayerBeginTouchWall(ushort playerId, ushort wallId, Vector2 wallNormalWhenTouchBegin, float wallRotationDegreesWhenTouchBegin, int tick);
        void OnPlayerEndTouchWall(ushort playerId, ushort wallId);
        List<PlayerStickToWallData> GetPlayersStickToWall(int currentTick, int minTicksTouching);
        void ClearAllData();
    }
}
