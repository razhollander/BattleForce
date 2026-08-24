using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate
{
    // Pure geometry for GatePass pass-detection and for talents that hit the gate through a cast. The gap is the
    // segment between the two posts; a player passes when the segment of his movement this tick properly crosses that
    // gap segment (in either direction).
    public static class ScoreGateGeometryUtils
    {
        public static void GetGapSegment(Vector2 gatePosition, Vector2 gateRotation, float gapHalfWidth, out Vector2 gapStart, out Vector2 gapEnd)
        {
            // gateRotation is the unit direction of the gate's local +X axis, which is also the axis the posts sit on,
            // so the clear gap runs along it from -gapHalfWidth to +gapHalfWidth around the gate centre.
            var gapAxis = gateRotation * gapHalfWidth;
            gapStart = gatePosition - gapAxis;
            gapEnd = gatePosition + gapAxis;
        }

        // Proper (strict) intersection of segment [p1,p2] with segment [q1,q2]. Collinear and endpoint-touch cases
        // return false on purpose: a player merely grazing the gap plane should not score.
        public static bool DoSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            var d1 = Cross(q2 - q1, p1 - q1);
            var d2 = Cross(q2 - q1, p2 - q1);
            var d3 = Cross(p2 - p1, q1 - p1);
            var d4 = Cross(p2 - p1, q2 - p1);

            var pStraddlesQ = (d1 > 0f && d2 < 0f) || (d1 < 0f && d2 > 0f);
            var qStraddlesP = (d3 > 0f && d4 < 0f) || (d3 < 0f && d4 > 0f);
            return pStraddlesQ && qStraddlesP;
        }

        // The gate's whole footprint - both posts plus the gap between them - as one rotated rectangle. A talent that
        // sweeps a shape over the gate has to shove it whether the sweep covers a post or passes through the gap,
        // and a shape cast only ever reports the two post fixtures, so the shove is tested against this footprint.
        public static bool DoesRotatedRectangleOverlapGate(Vector2 rectangleCenter, Vector2 rectangleHalfExtents, Vector2 rectangleAxis, Vector2 gatePosition, Vector2 gateRotation, Vector2 postSize, float gapWidth)
        {
            var gateHalfExtents = new Vector2(gapWidth * 0.5f + postSize.X, postSize.Y * 0.5f);
            return DoRotatedRectanglesOverlap(rectangleCenter, rectangleHalfExtents, rectangleAxis, gatePosition, gateHalfExtents, gateRotation);
        }

        // Separating axis test between two rotated rectangles. Each axis argument is the unit direction of that
        // rectangle's local +X; the rectangles overlap unless one of their four edge normals separates them.
        private static bool DoRotatedRectanglesOverlap(Vector2 centerA, Vector2 halfExtentsA, Vector2 axisA, Vector2 centerB, Vector2 halfExtentsB, Vector2 axisB)
        {
            var perpendicularAxisA = Perpendicular(axisA);
            var perpendicularAxisB = Perpendicular(axisB);

            var edgeAX = axisA * halfExtentsA.X;
            var edgeAY = perpendicularAxisA * halfExtentsA.Y;
            var edgeBX = axisB * halfExtentsB.X;
            var edgeBY = perpendicularAxisB * halfExtentsB.Y;

            var centerDelta = centerB - centerA;

            return !IsSeparatedOnAxis(axisA, centerDelta, edgeAX, edgeAY, edgeBX, edgeBY)
                   && !IsSeparatedOnAxis(perpendicularAxisA, centerDelta, edgeAX, edgeAY, edgeBX, edgeBY)
                   && !IsSeparatedOnAxis(axisB, centerDelta, edgeAX, edgeAY, edgeBX, edgeBY)
                   && !IsSeparatedOnAxis(perpendicularAxisB, centerDelta, edgeAX, edgeAY, edgeBX, edgeBY);
        }

        private static bool IsSeparatedOnAxis(Vector2 axis, Vector2 centerDelta, Vector2 edgeAX, Vector2 edgeAY, Vector2 edgeBX, Vector2 edgeBY)
        {
            var projectedRadiusA = MathF.Abs(Vector2.Dot(edgeAX, axis)) + MathF.Abs(Vector2.Dot(edgeAY, axis));
            var projectedRadiusB = MathF.Abs(Vector2.Dot(edgeBX, axis)) + MathF.Abs(Vector2.Dot(edgeBY, axis));
            return MathF.Abs(Vector2.Dot(centerDelta, axis)) > projectedRadiusA + projectedRadiusB;
        }

        private static Vector2 Perpendicular(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }

        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
