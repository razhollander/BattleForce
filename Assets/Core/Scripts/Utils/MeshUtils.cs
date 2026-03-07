using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshUtils
{


    /// <summary>
    /// Creates a 2D Sprite using the geometry of a 3D Mesh.
    /// </summary>
    /// <param name="sourceMesh">The procedural mesh you want to convert.</param>
    /// <param name="texture">The texture to apply to the sprite.</param>
    /// <param name="pixelsPerUnit">Unity's standard is 100 pixels per world unit.</param>
    /// <returns>A new Sprite with custom overridden geometry.</returns>
    public static Sprite ConvertMeshToSprite(Mesh sourceMesh, Texture2D texture, float pixelsPerUnit = 100f)
    {
        if (sourceMesh == null || texture == null)
        {
            Debug.LogError("Mesh or Texture is missing!");
            return null;
        }

        // 1. Create a base sprite to hold the data
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f); // Center pivot
        Sprite newSprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);

        // 2. Convert Vertices (Vector3[] -> Vector2[])
        Vector3[] meshVertices = sourceMesh.vertices;
        Vector2[] spriteVertices = new Vector2[meshVertices.Length];
        
        for (int i = 0; i < meshVertices.Length; i++)
        {
            spriteVertices[i] = new Vector2(meshVertices[i].x, meshVertices[i].y);
        }

        // 3. Convert Triangles (int[] -> ushort[])
        int[] meshTriangles = sourceMesh.triangles;
        ushort[] spriteTriangles = new ushort[meshTriangles.Length];
        
        for (int i = 0; i < meshTriangles.Length; i++)
        {
            // Unity Sprites have a strict limit of 65,535 vertices.
            // If your mesh exceeds this, the ushort cast will fail/wrap around.
            spriteTriangles[i] = (ushort)meshTriangles[i];
        }

        // 4. Inject the custom geometry into the Sprite
        newSprite.OverrideGeometry(spriteVertices, spriteTriangles);

        return newSprite;
    }

    /// <summary>
    /// Creates and returns a mesh from a polygon. Input should be CCW (will flip if CW).
    /// Throws if triangulation fails (usually self-intersection or duplicate/collinear issues).
    /// </summary>
    /// 
    public static Mesh BuildMesh(Vector2[] ccwPolygon, float z = 0f)
    {
        if (ccwPolygon == null) throw new ArgumentNullException(nameof(ccwPolygon));
        if (ccwPolygon.Length < 3) throw new ArgumentException("Polygon needs at least 3 points.");

        // Copy and sanitize (remove near-duplicate consecutive points).
        var poly = new List<Vector2>(ccwPolygon.Length);
        const float eps = 1e-6f;

        for (int i = 0; i < ccwPolygon.Length; i++)
        {
            var p = ccwPolygon[i];
            if (poly.Count == 0 || (poly[^1] - p).sqrMagnitude > eps)
                poly.Add(p);
        }
        // If last equals first (closed list), remove last
        if (poly.Count >= 2 && (poly[0] - poly[^1]).sqrMagnitude <= eps)
            poly.RemoveAt(poly.Count - 1);

        if (poly.Count < 3) throw new ArgumentException("Polygon became degenerate after cleanup.");

        // Ensure CCW winding (positive signed area).
        if (SignedArea(poly) < 0f)
            poly.Reverse();

        // Triangulate (ear clipping)
        var indices = TriangulateEarClipping(poly);
        if (indices.Count < 3)
            throw new InvalidOperationException("Triangulation failed. Polygon may be self-intersecting or invalid.");

        // Build mesh
        var mesh = new Mesh();
        var verts = new Vector3[poly.Count];
        var uvs = new Vector2[poly.Count];

        // Simple UVs: normalize into bounding box (0..1)
        Bounds2D b = Bounds2D.From(poly);
        Vector2 size = b.Size;
        if (Mathf.Abs(size.x) < eps) size.x = 1f;
        if (Mathf.Abs(size.y) < eps) size.y = 1f;

        for (int i = 0; i < poly.Count; i++)
        {
            verts[i] = new Vector3(poly[i].x, poly[i].y, z);
            uvs[i] = new Vector2((poly[i].x - b.Min.x) / size.x, (poly[i].y - b.Min.y) / size.y);
        }

        mesh.SetVertices(verts);
        mesh.SetUVs(0, new List<Vector2>(uvs));
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    // ----------------- Ear Clipping -----------------

    private static List<int> TriangulateEarClipping(List<Vector2> polyCCW)
    {
        int n = polyCCW.Count;
        var result = new List<int>((n - 2) * 3);
        if (n < 3) return result;

        // Vertex index list we "clip" from
        var V = new List<int>(n);
        for (int i = 0; i < n; i++) V.Add(i);

        int guard = 0;
        int maxGuard = n * n * 2; // safety

        int iCursor = 0;
        while (V.Count > 3)
        {
            if (++guard > maxGuard)
                throw new InvalidOperationException("Triangulation guard exceeded. Polygon likely invalid (self-intersecting?).");

            int prev = V[(iCursor - 1 + V.Count) % V.Count];
            int curr = V[iCursor % V.Count];
            int next = V[(iCursor + 1) % V.Count];

            Vector2 a = polyCCW[prev];
            Vector2 b = polyCCW[curr];
            Vector2 c = polyCCW[next];

            if (IsConvex(a, b, c)) // CCW convex
            {
                bool containsAny = false;
                for (int k = 0; k < V.Count; k++)
                {
                    int vi = V[k];
                    if (vi == prev || vi == curr || vi == next) continue;

                    if (PointInTriangle(polyCCW[vi], a, b, c))
                    {
                        containsAny = true;
                        break;
                    }
                }

                if (!containsAny)
                {
                    // Ear found
                    result.Add(prev);
                    result.Add(curr);
                    result.Add(next);
                    V.RemoveAt(iCursor % V.Count);
                    iCursor = Mathf.Max(0, iCursor - 1);
                    continue;
                }
            }

            iCursor = (iCursor + 1) % V.Count;
        }

        // Final triangle
        result.Add(V[0]);
        result.Add(V[1]);
        result.Add(V[2]);

        return result;
    }

    private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        // For CCW polygon, convex if cross(b-a, c-b) > 0
        return Cross(b - a, c - b) > 0f;
    }

    private static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // Barycentric technique; includes points on edges as "inside"
        float c1 = Cross(b - a, p - a);
        float c2 = Cross(c - b, p - b);
        float c3 = Cross(a - c, p - c);

        bool hasNeg = (c1 < 0f) || (c2 < 0f) || (c3 < 0f);
        bool hasPos = (c1 > 0f) || (c2 > 0f) || (c3 > 0f);
        return !(hasNeg && hasPos);
    }

    private static float SignedArea(List<Vector2> poly)
    {
        float a = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            var p = poly[i];
            var q = poly[(i + 1) % poly.Count];
            a += (p.x * q.y - q.x * p.y);
        }
        return a * 0.5f;
    }

    private readonly struct Bounds2D
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;
        public Vector2 Size => Max - Min;

        private Bounds2D(Vector2 min, Vector2 max) { Min = min; Max = max; }

        public static Bounds2D From(List<Vector2> pts)
        {
            Vector2 min = pts[0], max = pts[0];
            for (int i = 1; i < pts.Count; i++)
            {
                min = Vector2.Min(min, pts[i]);
                max = Vector2.Max(max, pts[i]);
            }
            return new Bounds2D(min, max);
        }
    }
}
