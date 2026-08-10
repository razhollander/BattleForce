using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Utils
{
    /// <summary>
    /// The whole gate trap cycle is derived from ticks, so the server and every client can run it from the same numbers:
    /// the server sends a single "closing until tick X" event and both sides play Closing -> Closed -> Opening -> Open
    /// out of the authored durations without any further traffic.
    /// </summary>
    public static class EnvironmentGateTrapUtils
    {
        public static int SecondsToTicks(float seconds, int ticksPerSecond)
        {
            if (seconds <= 0f)
            {
                return 0;
            }

            return (int)MathF.Ceiling(seconds * ticksPerSecond);
        }

        // A gate that only swings covers no distance, so for it MovementSpeed is read as degrees per second instead.
        public static float CalculateTransitionDurationInSeconds(Vector2 openPosition, Vector2 closedPosition, float openRotationDegrees, float closedRotationDegrees, float movementSpeed)
        {
            if (movementSpeed <= 0f)
            {
                return 0f;
            }

            var distance = Vector2.Distance(openPosition, closedPosition);
            var travel = distance > 0f ? distance : MathF.Abs(closedRotationDegrees - openRotationDegrees);
            return travel / movementSpeed;
        }

        public static int CalculateTransitionDurationInTicks(Vector2 openPosition, Vector2 closedPosition, float openRotationDegrees, float closedRotationDegrees, float movementSpeed, int ticksPerSecond)
        {
            var durationInSeconds = CalculateTransitionDurationInSeconds(openPosition, closedPosition, openRotationDegrees, closedRotationDegrees, movementSpeed);
            // A transition always spans at least one tick, otherwise the progress below would divide by zero.
            return Math.Max(1, SecondsToTicks(durationInSeconds, ticksPerSecond));
        }

        /// <summary>
        /// 0 is fully open, 1 is fully closed.
        /// </summary>
        public static float CalculateClosedProgress(GateTrapState state, int stateEndTick, int tick, int transitionDurationInTicks)
        {
            switch (state)
            {
                case GateTrapState.Closed:
                    return 1f;
                case GateTrapState.Closing:
                    return Clamp01(1f - (stateEndTick - tick) / (float)transitionDurationInTicks);
                case GateTrapState.Opening:
                    return Clamp01((stateEndTick - tick) / (float)transitionDurationInTicks);
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Runs every timed transition the trap owes up to the given tick. Only the Open state waits for something
        /// external (a player entering the area), so it is the loop's exit.
        /// </summary>
        public static void AdvanceTimedStates(ref GateTrapState state, ref int stateEndTick, int tick, int transitionDurationInTicks, int stayClosedDurationInTicks, int stayOpenDurationInTicks)
        {
            while (state != GateTrapState.Open && tick >= stateEndTick)
            {
                switch (state)
                {
                    case GateTrapState.Closing:
                        state = GateTrapState.Closed;
                        stateEndTick += stayClosedDurationInTicks;
                        break;
                    case GateTrapState.Closed:
                        state = GateTrapState.Opening;
                        stateEndTick += transitionDurationInTicks;
                        break;
                    case GateTrapState.Opening:
                        state = GateTrapState.Open;
                        stateEndTick += stayOpenDurationInTicks;
                        break;
                }
            }
        }

        // The trap can only be triggered once it is fully open and past its open cooldown - that cooldown is what the
        // client greys the wall out for.
        public static bool CanStartClosing(GateTrapState state, int stateEndTick, int tick)
        {
            return state == GateTrapState.Open && tick >= stateEndTick;
        }

        public static bool IsWaitingForOpenCooldown(GateTrapState state, int stateEndTick, int tick)
        {
            return state == GateTrapState.Open && tick < stateEndTick;
        }

        /// <summary>
        /// The pose is authored around a wall-local pivot, while both the physics body and the view rotate around their
        /// own origin, so the pivot is compensated for here: worldPoint = position + Rotate(point - pivot) + pivot.
        /// </summary>
        public static void CalculateWallTransform(Vector2 openPosition, Vector2 closedPosition, float openRotationDegrees, float closedRotationDegrees, Vector2 localRotationPivot, float closedProgress,
            out Vector2 position, out float rotationDegrees)
        {
            rotationDegrees = openRotationDegrees + (closedRotationDegrees - openRotationDegrees) * closedProgress;
            var pivotedPosition = openPosition + (closedPosition - openPosition) * closedProgress;
            position = pivotedPosition + localRotationPivot - localRotationPivot.Rotate(rotationDegrees);
        }

        public static bool IsPointInsidePolygon(ReadOnlySpan<Vector2> polygonPoints, Vector2 point)
        {
            var isInside = false;

            for (int i = 0, j = polygonPoints.Length - 1; i < polygonPoints.Length; j = i++)
            {
                var current = polygonPoints[i];
                var previous = polygonPoints[j];
                var doesEdgeCrossPointHeight = current.Y > point.Y != previous.Y > point.Y;

                if (!doesEdgeCrossPointHeight)
                {
                    continue;
                }

                var crossingX = (previous.X - current.X) * (point.Y - current.Y) / (previous.Y - current.Y) + current.X;
                if (point.X < crossingX)
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
