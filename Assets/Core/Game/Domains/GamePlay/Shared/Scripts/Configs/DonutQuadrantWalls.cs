using System;
using System.Collections.Generic;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    public static class DonutQuadrantWalls
    {
        /// <summary>
        /// Generates 4 JSON strings (Red, Green, Yellow, Blue) representing convex walls for each quadrant.
        /// Convex-only approach:
        /// - Builds the quadrant ring sector out of CONVEX QUADS (or triangles if innerRadius == 0).
        ///
        /// - outerRadius: outer ring radius
        /// - precision: number of segments per 90° quadrant (higher = smoother)
        /// Notes:
        /// - Inner radius is outerRadius * 0.5f.
        /// - Each wall uses 4 points (convex quad), so no concave polygons and under max 8 points.
        /// </summary>
        public static Dictionary<ushort, WallConfig[]> GenerateQuadrantWallPerTeam(float outerRadius, int precision)
        {
            if (outerRadius <= 0) throw new ArgumentOutOfRangeException(nameof(outerRadius));
            if (precision < 1) precision = 1;

            float innerRadius = outerRadius * 0.5f;

            // Quadrants (degrees)
            var quadrants = new (ushort TeamId, float StartDeg, float EndDeg)[]
            {
                (1,    90f,   0f),
                (2,   0f, -90f),
                (3, -90f,-180f),
                (4,  180f,  90f),
            };

            ushort nextId = 1;
            var result = new Dictionary<ushort, WallConfig[]>();

            foreach (var quadrant in quadrants)
            {
                // Convex quads per segment
                var walls = BuildConvexRingSectorQuads(
                    startDeg: quadrant.StartDeg,
                    endDeg: quadrant.EndDeg,
                    outerRadius: outerRadius,
                    innerRadius: innerRadius,
                    segments: precision,
                    ref nextId);

                result[quadrant.TeamId] = walls.ToArray();
            }

            return result;
        }

        /// <summary>
        /// Generates convex wrap-around circular wall (annulus ring) with radial thickness = width.
        /// Convex-only approach:
        /// - Builds the ring out of CONVEX QUADS per segment.
        ///
        /// - centerRadius: radius at the middle of the wall
        /// - width: radial thickness (outer = centerRadius + width/2, inner = centerRadius - width/2)
        /// - precision: number of segments around full 360°
        /// </summary>
        public static Vector2 GetTeamFloorCenter(ushort teamId, float outerRadius)
        {
            float innerRadius = outerRadius * 0.5f;
            float centerRadius = (outerRadius + innerRadius) * 0.5f;
            float angle = 0;

            switch (teamId)
            {
                case 1: angle = 45f; break;
                case 2: angle = -45f; break;
                case 3: angle = -135f; break;
                case 4: angle = 135f; break;
                default: return Vector2.Zero;
            }

            return PointOnCircle(centerRadius, angle);
        }

        public static WallConfig[] GenerateWrapAroundWallJson(float centerRadius, float width, int precision, ushort startId = 1000)
        {
            if (centerRadius <= 0) throw new ArgumentOutOfRangeException(nameof(centerRadius));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (precision < 4) precision = 4;

            float half = width * 0.5f;
            float innerRadius = centerRadius - half;
            float outerRadius = centerRadius + half;

            if (innerRadius <= 0)
                throw new ArgumentException("width is too large: innerRadius must remain > 0", nameof(width));

            ushort nextId = startId;

            // Build quads around full circle. Use 0..360 split into 'precision' segments.
            var walls = BuildConvexRingSectorQuads(
                startDeg: 0f,
                endDeg: 360f,
                outerRadius: outerRadius,
                innerRadius: innerRadius,
                segments: precision,
                ref nextId,
                wrapFullCircle: true);

            return walls.ToArray();
        }

        // --------------------------------------------------------------------
        // Convex builders
        // --------------------------------------------------------------------

        /// <summary>
        /// Builds a ring sector using convex quads, one per angular segment:
        /// Quad = [outer(a0), outer(a1), inner(a1), inner(a0)] (ordered consistently).
        ///
        /// Always convex when innerRadius > 0 and segment angle < 180° (true here).
        /// </summary>
        private static List<WallConfig> BuildConvexRingSectorQuads(
            float startDeg,
            float endDeg,
            float outerRadius,
            float innerRadius,
            int segments,
            ref ushort nextId,
            bool wrapFullCircle = false)
        {
            if (segments < 1) segments = 1;
            if (outerRadius <= 0) throw new ArgumentOutOfRangeException(nameof(outerRadius));
            if (innerRadius < 0) throw new ArgumentOutOfRangeException(nameof(innerRadius));
            if (innerRadius >= outerRadius) throw new ArgumentException("innerRadius must be < outerRadius");

            // For full circle, we do not include the duplicate endpoint.
            // We generate angles for each segment boundary.
            var angles = new List<float>(segments + 1);

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                angles.Add(Lerp(startDeg, endDeg, t));
            }

            if (wrapFullCircle)
            {
                // Remove the final angle (360) to avoid duplicating the 0 seam.
                // We'll connect last segment to the first by wrapping indices.
                angles.RemoveAt(angles.Count - 1);
            }

            var walls = new List<WallConfig>();

            int segCount = wrapFullCircle ? angles.Count : (angles.Count - 1);

            for (int i = 0; i < segCount; i++)
            {
                float a0 = angles[i];
                float a1 = wrapFullCircle ? angles[(i + 1) % angles.Count] : angles[i + 1];

                // Points on arcs
                var o0 = PointOnCircle(outerRadius, a0);
                var o1 = PointOnCircle(outerRadius, a1);

                if (innerRadius <= 0.0001f)
                {
                    // Degenerates into a triangle fan to center (still convex)
                    var tri = EnsureWindingCCW(new List<Vector2>
                    {
                        o0,
                        o1,
                        Vector2.Zero
                    });

                    walls.Add(new WallConfig(nextId++, tri.ToArray()));
                    continue;
                }

                var i1p = PointOnCircle(innerRadius, a1);
                var i0p = PointOnCircle(innerRadius, a0);

                // Convex quad: outer(a0) -> outer(a1) -> inner(a1) -> inner(a0)
                var quad = EnsureWindingCCW(new List<Vector2>
                {
                    o0, o1, i1p, i0p
                });

                // (Optional) Remove accidental duplicates at seam due to float noise
                quad = RemoveAdjacentDuplicates(quad);

                walls.Add(new WallConfig(nextId++, quad.ToArray()));
            }

            return walls;
        }

        // --------------------------------------------------------------------
        // Math helpers
        // --------------------------------------------------------------------

        private static Vector2 PointOnCircle(float r, float deg)
        {
            float rad = deg * (MathF.PI / 180f);
            return new Vector2
            {
                X = r * MathF.Cos(rad),
                Y = r * MathF.Sin(rad)
            };
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        /// <summary>
        /// Ensures polygon winding is CCW (helps some physics engines and keeps normals consistent).
        /// Works for triangles/quads here.
        /// </summary>
        private static List<Vector2> EnsureWindingCCW(List<Vector2> pts)
        {
            if (pts.Count < 3) return pts;

            // Signed area > 0 => CCW
            float area2 = 0f;
            for (int i = 0; i < pts.Count; i++)
            {
                var a = pts[i];
                var b = pts[(i + 1) % pts.Count];
                area2 += (a.X * b.Y - b.X * a.Y);
            }

            if (area2 < 0f)
                pts.Reverse();

            return pts;
        }

        private static List<Vector2> RemoveAdjacentDuplicates(List<Vector2> pts, float eps = 1e-5f)
        {
            if (pts.Count <= 1) return pts;

            var cleaned = new List<Vector2>(pts.Count) { pts[0] };

            for (int i = 1; i < pts.Count; i++)
            {
                var prev = cleaned[cleaned.Count - 1];
                var cur = pts[i];

                if (MathF.Abs(prev.X - cur.X) > eps || MathF.Abs(prev.Y - cur.Y) > eps)
                    cleaned.Add(cur);
            }

            // Also check last vs first (for closed polygon duplicates)
            if (cleaned.Count > 2)
            {
                var first = cleaned[0];
                var last = cleaned[cleaned.Count - 1];
                if (MathF.Abs(first.X - last.X) <= eps && MathF.Abs(first.Y - last.Y) <= eps)
                    cleaned.RemoveAt(cleaned.Count - 1);
            }

            return cleaned;
        }
    }
}
