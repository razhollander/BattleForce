using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall
{
    public readonly struct PlayerStickToWallData
    {
        public readonly ushort PlayerId;
        public readonly ushort WallId;

        /// <summary>
        /// The contact normal expressed relative to the wall's own rotation, so it stays valid even if the wall
        /// rotates afterwards (e.g. a wall attached to a rotating wheel). Rotate by the wall's current
        /// WorldRotationDegrees to get the current world-space normal.
        /// </summary>
        public readonly Vector2 WallLocalNormal;

        public PlayerStickToWallData(ushort playerId, ushort wallId, Vector2 wallLocalNormal)
        {
            PlayerId = playerId;
            WallId = wallId;
            WallLocalNormal = wallLocalNormal;
        }
    }
}
