using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall
{
    public readonly struct PlayerStickToWallData
    {
        public readonly ushort PlayerId;
        public readonly Vector2 WallNormal;

        public PlayerStickToWallData(ushort playerId, Vector2 wallNormal)
        {
            PlayerId = playerId;
            WallNormal = wallNormal;
        }
    }
}
