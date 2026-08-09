using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate
{
    // Pure geometry for GatePass pass-detection. The gap is the segment between the two posts; a player passes when the
    // segment of his movement this tick properly crosses that gap segment (in either direction).
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

        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
