// PolygonPath2D.cs
using System.Collections.Generic;
using System.Linq;
using CoreDomain.Scripts.Extensions;
using UnityEngine;

public class PolygonPath2D : MonoBehaviour
{
    [SerializeField] private List<Vector2> points = new();
    [SerializeField] private bool closed = true;
    [SerializeField] public MeshFilter MeshFilter;
    [SerializeField] public MeshRenderer MeshRenderer;
    public List<Vector2> Points => points;

    public List<Vector2> GetPointsRelativeToObject()
    {
        return points.Select(x=>x+transform.position.ToVector2XY()).ToList();
    }
    public bool Closed { get => closed; set => closed = value; }

    public Vector2[] GetPointsCCW()
    {
        if (points == null || points.Count < 3) return points?.ToArray() ?? new Vector2[0];
        var arr = points.ToArray();
        if (SignedArea(arr) < 0f) System.Array.Reverse(arr); // flip CW -> CCW
        return arr;
    }

    static float SignedArea(Vector2[] poly)
    {
        float a = 0f;
        for (int i = 0; i < poly.Length; i++)
        {
            var p = poly[i];
            var q = poly[(i + 1) % poly.Length];
            a += (p.x * q.y - q.x * p.y);
        }
        return a * 0.5f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (points == null || points.Count == 0) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;

        for (int i = 0; i < points.Count - 1; i++)
            Gizmos.DrawLine(points[i], points[i + 1]);

        if (closed && points.Count >= 3)
            Gizmos.DrawLine(points[^1], points[0]);

        foreach (var p in points)
            Gizmos.DrawSphere(p, 0.03f);
    }
#endif
}