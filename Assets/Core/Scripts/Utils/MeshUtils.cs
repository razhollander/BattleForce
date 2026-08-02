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

    public static Mesh CreateRectangleMesh(System.Numerics.Vector2 size, float z = 0f)
    {
        var halfSize = size * 0.5f;

        var points = new[]
        {
            new Vector2(-halfSize.X, -halfSize.Y),
            new Vector2(halfSize.X, -halfSize.Y),
            new Vector2(halfSize.X, halfSize.Y),
            new Vector2(-halfSize.X, halfSize.Y)
        };

        return MeshUtils.BuildMesh(points, z);
    }
    
    public static Mesh CreateCircleMesh(float radius, float thickness, int circleSegments, float z = 0f)
    {
        var totalSegments = circleSegments;
        var halfSegments = totalSegments / 2;

        var innerRadius = radius;
        var outerRadius = radius + thickness;
            
        // we create 2 half circles and not 1 single full circle because afterwards the triangluation doesn't support overlapping points
        var topHalfPoints = GenerateHalfRingPoints(0f, 180f, halfSegments, innerRadius, outerRadius);
        var topHalfMesh = MeshUtils.BuildMesh(topHalfPoints, z);
            
        var bottomHalfPoints = GenerateHalfRingPoints(180f, 360f, halfSegments, innerRadius, outerRadius);
        var bottomHalfMesh = MeshUtils.BuildMesh(bottomHalfPoints, z);
            
        var combine = new CombineInstance[2];

        combine[0].mesh = topHalfMesh;
        combine[0].transform = Matrix4x4.identity;

        combine[1].mesh = bottomHalfMesh;
        combine[1].transform = Matrix4x4.identity;

        var finalMesh = new Mesh();
        finalMesh.CombineMeshes(combine, true, false);

        UnityEngine.Object.Destroy(topHalfMesh);
        UnityEngine.Object.Destroy(bottomHalfMesh);

        return finalMesh;
    }
    
    /// <summary>
    /// Creates a 2D cone (sector / pie-slice) mesh. The apex sits at the origin and the arc
    /// is swept symmetrically around the +X axis, spanning <paramref name="openingAngle"/> total degrees.
    /// The surface is subdivided both along the arc (<paramref name="arcSegments"/>) and radially
    /// (<paramref name="radialSegments"/>) into concentric rings, producing evenly spread triangles
    /// instead of thin slivers meeting at the apex.
    /// </summary>
    /// <param name="radius">Distance from the apex to the arc.</param>
    /// <param name="openingAngle">Total opening angle of the cone in degrees.</param>
    /// <param name="arcSegments">Number of segments used to approximate the arc.</param>
    /// <param name="radialSegments">Number of concentric rings from the apex to the arc.</param>
    /// <param name="z">Z offset for the generated vertices.</param>
    public static Mesh CreateConeMesh(float radius, float openingAngle, int arcSegments = 16, int radialSegments = 8, float z = 0f)
    {
        if (arcSegments < 1) arcSegments = 1;
        if (radialSegments < 1) radialSegments = 1;

        var halfAngle = openingAngle * 0.5f;
        var startAngle = -halfAngle;
        var endAngle = halfAngle;

        int arcPoints = arcSegments + 1;

        // Apex (shared single vertex) + a full arc of points per radial ring (rings 1..radialSegments).
        var verts = new Vector3[1 + radialSegments * arcPoints];
        var uvs = new Vector2[verts.Length];
        verts[0] = new Vector3(0f, 0f, z);

        for (int r = 1; r <= radialSegments; r++)
        {
            float ringRadius = radius * ((float) r / radialSegments);
            int ringStart = 1 + (r - 1) * arcPoints;

            for (int a = 0; a < arcPoints; a++)
            {
                float t = (float) a / arcSegments;
                float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                verts[ringStart + a] = new Vector3(Mathf.Cos(angle) * ringRadius, Mathf.Sin(angle) * ringRadius, z);
            }
        }

        // UVs: normalize the (x, y) footprint into 0..1 so any texture maps sensibly.
        float minX = -radius, minY = -radius * Mathf.Sin(halfAngle * Mathf.Deg2Rad);
        float sizeX = radius - minX;
        float sizeY = (radius * Mathf.Sin(halfAngle * Mathf.Deg2Rad)) - minY;
        if (Mathf.Abs(sizeX) < 1e-6f) sizeX = 1f;
        if (Mathf.Abs(sizeY) < 1e-6f) sizeY = 1f;
        for (int i = 0; i < verts.Length; i++)
            uvs[i] = new Vector2((verts[i].x - minX) / sizeX, (verts[i].y - minY) / sizeY);

        // Triangles. Inner ring (apex fan) + a quad grid between successive rings.
        var triangles = new int[(arcSegments + (radialSegments - 1) * arcSegments * 2) * 3];
        int ti = 0;

        // Apex fan to first ring (CCW when viewed from +Z).
        int firstRingStart = 1;
        for (int a = 0; a < arcSegments; a++)
        {
            triangles[ti++] = 0;
            triangles[ti++] = firstRingStart + a;
            triangles[ti++] = firstRingStart + a + 1;
        }

        // Quad grid between ring r and ring r+1.
        for (int r = 1; r < radialSegments; r++)
        {
            int innerStart = 1 + (r - 1) * arcPoints;
            int outerStart = 1 + r * arcPoints;

            for (int a = 0; a < arcSegments; a++)
            {
                int aInner = innerStart + a;
                int bInner = innerStart + a + 1;
                int aOuter = outerStart + a;
                int bOuter = outerStart + a + 1;

                triangles[ti++] = aInner;
                triangles[ti++] = aOuter;
                triangles[ti++] = bOuter;

                triangles[ti++] = aInner;
                triangles[ti++] = bOuter;
                triangles[ti++] = bInner;
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>
    /// Creates a solid 3D cone: the surface of revolution of the 2D cone around its central (+X) axis.
    /// The apex sits at the origin and the cone opens toward +X. <paramref name="radius"/> is the slant
    /// distance from the apex to the rim and <paramref name="openingAngle"/> is the full apex angle, so
    /// they keep the exact same meaning as <see cref="CreateConeMesh"/>. The lateral surface is subdivided
    /// evenly both around the circumference (<paramref name="radialSegments"/>) and along the slant
    /// (<paramref name="slantSegments"/>).
    /// </summary>
    /// <param name="radius">Slant distance from the apex to the base rim.</param>
    /// <param name="openingAngle">Total apex angle in degrees.</param>
    /// <param name="radialSegments">Number of segments around the circumference.</param>
    /// <param name="slantSegments">Number of rings from the apex to the base rim.</param>
    /// <param name="addBaseCap">Whether to close the base with a disk.</param>
    public static Mesh CreateConeMesh3D(float radius, float openingAngle, int radialSegments = 24, int slantSegments = 4, bool addBaseCap = true)
    {
        if (radialSegments < 3) radialSegments = 3;
        if (slantSegments < 1) slantSegments = 1;

        float halfAngle = openingAngle * 0.5f * Mathf.Deg2Rad;
        float axisCos = Mathf.Cos(halfAngle); // spread along +X per unit slant
        float radialSin = Mathf.Sin(halfAngle); // spread away from the axis per unit slant

        // --- Lateral surface vertices: apex + one circle per slant ring ---
        int lateralCount = 1 + slantSegments * radialSegments;
        int capCount = addBaseCap ? 1 + radialSegments : 0;

        var verts = new Vector3[lateralCount + capCount];
        var uvs = new Vector2[verts.Length];

        verts[0] = Vector3.zero; // apex
        uvs[0] = new Vector2(0.5f, 0f);

        for (int r = 1; r <= slantSegments; r++)
        {
            float slant = radius * ((float) r / slantSegments);
            float x = slant * axisCos;
            float ringRadius = slant * radialSin;
            int ringStart = 1 + (r - 1) * radialSegments;

            for (int s = 0; s < radialSegments; s++)
            {
                float phi = (float) s / radialSegments * Mathf.PI * 2f;
                verts[ringStart + s] = new Vector3(x, Mathf.Cos(phi) * ringRadius, Mathf.Sin(phi) * ringRadius);
                uvs[ringStart + s] = new Vector2((float) s / radialSegments, (float) r / slantSegments);
            }
        }

        // --- Base cap vertices (duplicated rim for a sharp edge + correct +X normals) ---
        int capCenter = lateralCount;
        if (addBaseCap)
        {
            float xBase = radius * axisCos;
            float baseRadius = radius * radialSin;
            verts[capCenter] = new Vector3(xBase, 0f, 0f);
            uvs[capCenter] = new Vector2(0.5f, 0.5f);

            for (int s = 0; s < radialSegments; s++)
            {
                float phi = (float) s / radialSegments * Mathf.PI * 2f;
                verts[capCenter + 1 + s] = new Vector3(xBase, Mathf.Cos(phi) * baseRadius, Mathf.Sin(phi) * baseRadius);
                uvs[capCenter + 1 + s] = new Vector2(0.5f + 0.5f * Mathf.Cos(phi), 0.5f + 0.5f * Mathf.Sin(phi));
            }
        }

        // --- Triangles (wound so the outside faces outward) ---
        int lateralTris = radialSegments + (slantSegments - 1) * radialSegments * 2;
        int capTris = addBaseCap ? radialSegments : 0;
        var triangles = new int[(lateralTris + capTris) * 3];
        int ti = 0;

        // Apex fan to the first ring.
        int first = 1;
        for (int s = 0; s < radialSegments; s++)
        {
            int next = (s + 1) % radialSegments;
            triangles[ti++] = 0;
            triangles[ti++] = first + next;
            triangles[ti++] = first + s;
        }

        // Quad grid between successive rings.
        for (int r = 1; r < slantSegments; r++)
        {
            int innerStart = 1 + (r - 1) * radialSegments;
            int outerStart = 1 + r * radialSegments;

            for (int s = 0; s < radialSegments; s++)
            {
                int next = (s + 1) % radialSegments;
                int innerCurr = innerStart + s;
                int innerNext = innerStart + next;
                int outerCurr = outerStart + s;
                int outerNext = outerStart + next;

                triangles[ti++] = innerCurr;
                triangles[ti++] = outerNext;
                triangles[ti++] = outerCurr;

                triangles[ti++] = innerCurr;
                triangles[ti++] = innerNext;
                triangles[ti++] = outerNext;
            }
        }

        // Base cap fan (outward normal points +X).
        if (addBaseCap)
        {
            for (int s = 0; s < radialSegments; s++)
            {
                int next = (s + 1) % radialSegments;
                triangles[ti++] = capCenter;
                triangles[ti++] = capCenter + 1 + s;
                triangles[ti++] = capCenter + 1 + next;
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Vector2[] GenerateHalfRingPoints(float startAngle, float endAngle, int segments, float innerRadius, float outerRadius)
    {
        // A half ring needs (segments + 1) for the outer edge, and (segments + 1) for the inner edge
        var points = new Vector2[(segments + 1) * 2];

        // Outer Arc (Counter-Clockwise)
        for (int i = 0; i <= segments; i++)
        {
            // Calculate interpolation factor (0.0 to 1.0)
            float t = (float) i / segments;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
            points[i] = new Vector2(Mathf.Cos(angle) * outerRadius, Mathf.Sin(angle) * outerRadius);
        }

        // Inner Arc (Clockwise)
        for (int i = 0; i <= segments; i++)
        {
            // Calculate interpolation factor backwards (1.0 to 0.0)
            float t = (float) (segments - i) / segments;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;

            // Offset by (segments + 1) to place them in the second half of the array
            points[(segments + 1) + i] = new Vector2(Mathf.Cos(angle) * innerRadius, Mathf.Sin(angle) * innerRadius);
        }

        return points;
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
