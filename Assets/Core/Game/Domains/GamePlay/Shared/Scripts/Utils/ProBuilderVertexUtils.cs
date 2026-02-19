using System;
using System.Linq;
using UnityEngine.ProBuilder;
using System.Numerics;
public static class ProBuilderVertexUtils
{
    /// <summary>
    /// Returns the mesh's outer polygon vertices in anti-clockwise order in XY.
    /// Assumes the ProBuilderMesh is truly 2D in the XY plane (Z is constant / irrelevant).
    /// Uses a convex hull (robust) and ensures CCW winding.
    /// Notes (important for Box2D):
    /// This returns the convex hull of the ProBuilder vertices in CCW order (good for Box2D polygons).
    /// If your shape is concave, Box2D can’t take it as one polygon anyway — you must split it into convex pieces.
    /// Box2D typically has a max 8 vertices limit per polygon shape, so if the hull returns more than 8, you’ll need to simplify/split.
    /// </summary>
    public static Vector2[] GetVerticesCCW_XY(ProBuilderMesh pb)
    {
        if (pb == null) throw new ArgumentNullException(nameof(pb));

        // ProBuilder vertices are not in polygon order; take all positions and compute hull.
        var pos = pb.positions;
        if (pos == null || pos.Count == 0) return Array.Empty<Vector2>();

        // Convert to 2D (XY), de-duplicate with a small tolerance by rounding.
        // (Unity/ProBuilder can have near-duplicates.)
        const float round = 1e-5f;
        Vector2[] pts = pos
            .Select(v => new Vector2(v.x, v.y))
            .Select(v => new Vector2(
                UnityEngine.Mathf.Round(v.X / round) * round,
                UnityEngine.Mathf.Round(v.Y / round) * round))
            .Distinct()
            .ToArray();

        if (pts.Length <= 2) return pts;

        // --- Andrew's monotonic chain convex hull (returns CCW) ---
        var sorted = pts.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();

        Vector2[] hull = BuildHull(sorted);
        if (hull.Length <= 2) return hull;

        // Ensure CCW winding (just in case)
        if (SignedArea(hull) < 0f) // negative means CW for this area formula
            Array.Reverse(hull);

        return hull;

        static Vector2[] BuildHull(Vector2[] p)
        {
            var lower = new System.Collections.Generic.List<Vector2>();
            foreach (var pt in p)
            {
                while (lower.Count >= 2 && Cross(lower[^2], lower[^1], pt) <= 0f)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(pt);
            }

            var upper = new System.Collections.Generic.List<Vector2>();
            for (int i = p.Length - 1; i >= 0; i--)
            {
                var pt = p[i];
                while (upper.Count >= 2 && Cross(upper[^2], upper[^1], pt) <= 0f)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(pt);
            }

            // Remove last because it repeats the first point of the other half
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);

            return lower.Concat(upper).ToArray(); // CCW hull
        }

        static float Cross(in Vector2 a, in Vector2 b, in Vector2 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        static float SignedArea(Vector2[] poly)
        {
            float area2 = 0f;
            for (int i = 0; i < poly.Length; i++)
            {
                Vector2 a = poly[i];
                Vector2 b = poly[(i + 1) % poly.Length];
                area2 += (a.X * b.Y - b.X * a.Y);
            }
            return area2 * 0.5f;
        }
    }
}
