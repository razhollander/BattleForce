using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    /// <summary>
    /// An environment trap made of a sensing area (GateTrapAreaPolygonConfig list) and a single moving wall.
    /// The wall itself is authored as a regular environment wall so it collides exactly like one - the trap only drives
    /// its transform between the authored open and closed poses.
    /// </summary>
    [Serializable]
    public class EnvironmentGateTrapConfig : IEquatable<ushort>
    {
        public ushort Id;

        // The wall this trap drives. Its id shares the layout's wall id space, so it must not clash with an authored wall.
        public ushort WallId;
        public Vector2[] WallPoints;

        public GateTrapAreaPolygonConfig[] AreaPolygons;

        public Vector2 OpenPosition;
        public Vector2 ClosedPosition;
        public float OpenRotationDegrees;
        public float ClosedRotationDegrees;

        // Wall-local point the open/closed rotation turns around, letting a gate swing on its hinge instead of its centre.
        public Vector2 LocalRotationPivot;

        // Units per second travelled between the open and closed positions. A gate that only swings has no distance to
        // cover, so for it the same value is read as degrees per second.
        public float MovementSpeed;

        public float SecondsStayClosed;
        public float SecondsStayOpen;

        public bool IsAttachedToRotationWheel;
        public ushort AttachToRotationWheelId;

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }

    /// <summary>
    /// One convex piece of a trap's sensing area. Points are authored in the same space as the trap's wall positions,
    /// so a trap attached to a rotating wheel senses in wheel-local space and rotates together with its wall.
    /// </summary>
    [Serializable]
    public class GateTrapAreaPolygonConfig
    {
        public const int MAX_POINTS = 8;

        public Vector2[] Points;
    }
}
