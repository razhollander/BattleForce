using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    /// <summary>
    /// The map-scaled, tick-based form of an EnvironmentGateTrapConfig. Everything here is fixed for the stage - the
    /// live state (Open/Closing/Closed/Opening plus its end tick) lives in the simulation state so it reaches a
    /// rejoining client.
    /// </summary>
    public class MatchEnvironmentGateTrapModel : IEquatable<ushort>
    {
        public ushort Id;

        // The environment wall this trap drives. It is a regular wall body, so it blocks and bounces like any other.
        public ushort WallId;

        public Vector2 OpenPosition;
        public Vector2 ClosedPosition;
        public float OpenRotationDegrees;
        public float ClosedRotationDegrees;
        public Vector2 LocalRotationPivot;

        public int TransitionDurationInTicks;
        public int StayClosedDurationInTicks;
        public int StayOpenDurationInTicks;

        public bool IsAttachedToRotationWheel;
        public ushort AttachedToRotationWheelId;

        // Sensing area in the trap's own space: world space for a free trap, wheel-local space for one on a wheel.
        public Vector2[][] AreaPolygons;

        public bool IsAnyPointInsideArea(Vector2 point)
        {
            if (AreaPolygons == null)
            {
                return false;
            }

            foreach (var polygonPoints in AreaPolygons)
            {
                if (EnvironmentGateTrapUtils.IsPointInsidePolygon(polygonPoints, point))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
