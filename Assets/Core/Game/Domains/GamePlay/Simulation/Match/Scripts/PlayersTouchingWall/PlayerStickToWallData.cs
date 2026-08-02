using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall
{
    public readonly struct PlayerStickToWallData
    {
        public readonly ushort PlayerId;

        /// <summary>
        /// Which kind of wall-like body is being touched (a static Wall or a FrigidBlock). Wall ids and FrigidBlock ids
        /// come from separate id spaces, so the body type is needed both to tell two touches apart and to know where to
        /// read the body's current rotation from.
        /// </summary>
        public readonly PhysicsBodyType WallBodyType;

        public readonly ushort WallId;

        /// <summary>
        /// The contact normal expressed relative to the wall's own rotation, so it stays valid even if the wall
        /// rotates afterwards (e.g. a wall attached to a rotating wheel). Rotate by the wall's current
        /// WorldRotationDegrees to get the current world-space normal.
        /// </summary>
        public readonly Vector2 WallLocalNormal;

        public PlayerStickToWallData(ushort playerId, PhysicsBodyType wallBodyType, ushort wallId, Vector2 wallLocalNormal)
        {
            PlayerId = playerId;
            WallBodyType = wallBodyType;
            WallId = wallId;
            WallLocalNormal = wallLocalNormal;
        }
    }
}
