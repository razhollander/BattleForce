using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    public static class DonutQuadrantWalls
    {
        /// <summary>
        /// Generates 4 JSON strings (Red, Green, Yellow, Blue) representing donut-quadrant walls.
        /// - outerRadius: outer ring radius
        /// - precision: how many segments per 90° quadrant (points = precision+1 along each arc)
        /// Notes:
        /// - Inner radius is outerRadius * 0.5f (matches the image's "hole" vibe). Change if needed.
        /// - Max points per wall is 8. If needed, each quadrant is split into sub-walls.
        /// - Returned dictionary keys: "Red", "Green", "Yellow", "Blue"
        /// </summary>
        public static Dictionary<string, string> Generate4QuadrantWallJsons(float outerRadius, int precision)
        {
            if (outerRadius <= 0) throw new ArgumentOutOfRangeException(nameof(outerRadius));
            if (precision < 1) precision = 1;

            float innerRadius = outerRadius * 0.5f;

            // Quadrants (degrees), clockwise-ish to match common 2D shapes:
            // Red:   90 -> 0
            // Green: 0  -> -90
            // Yellow:-90-> -180
            // Blue:  180-> 90
            var quadrants = new (string Name, float StartDeg, float EndDeg)[]
            {
                ("Red", 90f, 0f),
                ("Green", 0f, -90f),
                ("Yellow", -90f, -180f),
                ("Blue", 180f, 90f),
            };

            ushort nextId = 1;
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var q in quadrants)
            {
                var walls = BuildQuadrantWalls(
                    startDeg: q.StartDeg,
                    endDeg: q.EndDeg,
                    outerRadius: outerRadius,
                    innerRadius: innerRadius,
                    precision: precision,
                    ref nextId);

                // Serialize each color's walls as its own JSON (array of walls)
                result[q.Name] = walls.ToJson();
            }

            return result;
        }

        // ---- Implementation details ----

        private static List<WallConfig> BuildQuadrantWalls(
            float startDeg,
            float endDeg,
            float outerRadius,
            float innerRadius,
            int precision,
            ref ushort nextId)
        {
            // Outer arc points (including endpoints)
            var outerArc = SampleArcPoints(outerRadius, startDeg, endDeg, precision);

            // We must cap polygon points to 8.
            // A ring-sector polygon = outerChunkCount + innerChunkCount (same count reversed) => 2*outerChunkCount points.
            // So outerChunkCount must be <= 4.
            const int maxPointsPerWall = 8;
            int maxOuterPtsPerWall = maxPointsPerWall / 2; // 4

            // Split into chunks, overlapping by 1 point so there are no gaps between sub-walls.
            // Each chunk uses outer points [i..j] inclusive.
            var walls = new List<WallConfig>();
            int i = 0;

            while (i < outerArc.Count - 1) // need at least 2 outer points per wall
            {
                int remaining = outerArc.Count - i;
                int take = Math.Min(maxOuterPtsPerWall, remaining);
                if (take < 2) take = 2;

                int j = i + take - 1;
                if (j >= outerArc.Count) j = outerArc.Count - 1;

                var outerChunk = outerArc.GetRange(i, j - i + 1);

                // Inner arc chunk uses same angles but at inner radius.
                // Build corresponding inner chunk by sampling the exact same degrees used by outerChunk.
                var degreesForChunk = outerChunk.Select(p => MathF.Atan2(p.Y, p.X) * (180f / MathF.PI)).ToList();
                var innerChunk = degreesForChunk.Select(d => PointOnCircle(innerRadius, d)).ToList();

                // Polygon points: outer chunk in order + inner chunk in reverse order
                var poly = new List<Vector2>(outerChunk.Count + innerChunk.Count);
                poly.AddRange(outerChunk);
                innerChunk.Reverse();
                poly.AddRange(innerChunk);

                // Remove any adjacent duplicates (can happen at boundaries)
                poly = RemoveAdjacentDuplicates(poly);

                // If we somehow still exceed, shrink (shouldn't happen with chunking)
                if (poly.Count > maxPointsPerWall)
                    poly = poly.Take(maxPointsPerWall).ToList();

                walls.Add(new WallConfig(nextId++, poly.ToArray()));

                // Move to next chunk with 1-point overlap
                i = j;
            }

            return walls;
        }

        private static List<Vector2> SampleArcPoints(float radius, float startDeg, float endDeg, int precision)
        {
            // precision = segments per quadrant arc. Points = precision+1.
            int points = precision + 1;

            var list = new List<Vector2>(points);

            for (int k = 0; k < points; k++)
            {
                float t = (points == 1) ? 0f : (k / (float) (points - 1));
                float deg = Lerp(startDeg, endDeg, t);
                list.Add(PointOnCircle(radius, deg));
            }

            return list;
        }

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

        private static List<Vector2> RemoveAdjacentDuplicates(List<Vector2> pts, float eps = 1e-5f)
        {
            if (pts.Count <= 1) return pts;

            var cleaned = new List<Vector2>(pts.Count) {pts[0]};

            for (int i = 1; i < pts.Count; i++)
            {
                var prev = cleaned[cleaned.Count - 1];
                var cur = pts[i];

                if (MathF.Abs(prev.X - cur.X) > eps || MathF.Abs(prev.Y - cur.Y) > eps)
                    cleaned.Add(cur);
            }

            return cleaned;
        }

        //
        // private static readonly JsonSerializerOptions JsonOptions = new()
        // {
        //     WriteIndented = true,
        //     DefaultIgnoreCondition = JsonIgnoreCondition.Never
        // };

        /// <summary>
        /// Generates a "wrap-around" circular wall (an annulus ring) centered at (0,0).
        /// The wall has radial thickness = width.
        ///
        /// - centerRadius: radius at the middle of the wall (think: track centerline)
        /// - width: radial thickness (outer = centerRadius + width/2, inner = centerRadius - width/2)
        /// - precision: number of segments around 360° (points per arc = precision+1; higher = smoother)
        ///
        /// Output:
        /// - JSON array of walls (sub-walls) because each wall is capped to 8 points.
        ///
        /// Note:
        /// - This builds polygons for full ring by splitting into chunks so (outerPts + innerPts) <= 8.
        /// </summary>
        public static WallConfig[] GenerateWrapAroundWallJson(float centerRadius, float width, int precision, ushort startId = 1000)
        {
            if (centerRadius <= 0) throw new ArgumentOutOfRangeException(nameof(centerRadius));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (precision < 4) precision = 4; // reasonable minimum for a circle

            float half = width * 0.5f;
            float innerRadius = centerRadius - half;
            float outerRadius = centerRadius + half;

            if (innerRadius <= 0)
                throw new ArgumentException("width is too large: innerRadius must remain > 0", nameof(width));

            ushort nextId = startId;

            // We build the full circle from 0 -> 360 (inclusive endpoint duplicates 0°).
            // We'll treat it as a closed loop but avoid duplicate last point in chunking.
            var outerArc = SampleArcPoints(outerRadius, startDeg: 0f, endDeg: 360f, precision);

            // Remove the final point at 360° (duplicate of 0°) to avoid zero-length edges.
            if (outerArc.Count > 1)
                outerArc.RemoveAt(outerArc.Count - 1);

            // Each ring polygon chunk: outerChunkCount + innerChunkCount, where innerChunkCount matches outerChunkCount.
            // With max 8 points total => outerChunkCount <= 4.
            const int maxPointsPerWall = 8;
            int maxOuterPtsPerWall = maxPointsPerWall / 2; // 4

            var walls = new List<WallConfig>();

            // Chunk with overlap to keep continuity.
            int i = 0;

            while (i < outerArc.Count)
            {
                int remaining = outerArc.Count - i;

                // Need at least 2 points for a polygon strip.
                int take = Math.Min(maxOuterPtsPerWall, remaining);

                if (take < 2)
                {
                    // Wrap remainder by taking from start to make at least 2 points
                    // (only happens when remaining == 1)
                    take = 2;
                }

                // Collect indices (wrapping around)
                var outerChunk = new List<Vector2>(take);

                for (int k = 0; k < take; k++)
                    outerChunk.Add(outerArc[(i + k) % outerArc.Count]);

                // Inner chunk uses same angles as outer points
                var degreesForChunk = outerChunk
                    .Select(p => MathF.Atan2(p.Y, p.X) * (180f / MathF.PI))
                    .ToList();

                var innerChunk = degreesForChunk.Select(d => PointOnCircle(innerRadius, d)).ToList();

                // Polygon: outer forward + inner reverse
                var poly = new List<Vector2>(outerChunk.Count + innerChunk.Count);
                poly.AddRange(outerChunk);
                innerChunk.Reverse();
                poly.AddRange(innerChunk);

                poly = RemoveAdjacentDuplicates(poly);

                // Ensure <= 8 (should be by construction)
                if (poly.Count > maxPointsPerWall)
                    poly = poly.Take(maxPointsPerWall).ToList();

                walls.Add(new WallConfig(nextId++, poly.ToArray()));

                // Advance with 1-point overlap (wrap safe)
                i += (take - 1);

                // Stop condition: if we’ve looped enough to cover full circle.
                // Since we’re incrementing with overlap, end when i >= outerArc.Count.
                if (i >= outerArc.Count)
                    break;
            }

            return walls.ToArray();
        }

        // ---- Shared helpers ----

        private static List<WallConfig> BuildRingSectorWalls(
            float startDeg,
            float endDeg,
            float outerRadius,
            float innerRadius,
            int precision,
            ref ushort nextId)
        {
            var outerArc = SampleArcPoints(outerRadius, startDeg, endDeg, precision);

            const int maxPointsPerWall = 8;
            int maxOuterPtsPerWall = maxPointsPerWall / 2; // 4

            var walls = new List<WallConfig>();
            int i = 0;

            while (i < outerArc.Count - 1) // need at least 2 outer points per wall
            {
                int remaining = outerArc.Count - i;
                int take = Math.Min(maxOuterPtsPerWall, remaining);
                if (take < 2) take = 2;

                int j = i + take - 1;
                if (j >= outerArc.Count) j = outerArc.Count - 1;

                var outerChunk = outerArc.GetRange(i, j - i + 1);

                var degreesForChunk = outerChunk.Select(p => MathF.Atan2(p.Y, p.X) * (180f / MathF.PI)).ToList();
                var innerChunk = degreesForChunk.Select(d => PointOnCircle(innerRadius, d)).ToList();

                var poly = new List<Vector2>(outerChunk.Count + innerChunk.Count);
                poly.AddRange(outerChunk);
                innerChunk.Reverse();
                poly.AddRange(innerChunk);

                poly = RemoveAdjacentDuplicates(poly);

                if (poly.Count > maxPointsPerWall)
                    poly = poly.Take(maxPointsPerWall).ToList();

                walls.Add(new WallConfig(nextId++, poly.ToArray()));

                // overlap by one point
                i = j;
            }

            return walls;
        }
    }
}
