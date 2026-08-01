using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall
{
    public interface IPlayersTouchingWallDataService
    {
        void OnPlayerBeginTouchWall(ushort playerId, PhysicsBodyType wallBodyType, ushort wallId, Vector2 wallNormalWhenTouchBegin, float wallRotationDegreesWhenTouchBegin, int tick);
        void OnPlayerEndTouchWall(ushort playerId, PhysicsBodyType wallBodyType, ushort wallId);
        List<PlayerStickToWallData> GetPlayersStickToWall(int currentTick, int minTicksTouching);
        void ClearAllData();
    }
}
