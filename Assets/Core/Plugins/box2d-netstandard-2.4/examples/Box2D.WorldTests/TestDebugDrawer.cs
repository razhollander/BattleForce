using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using UnityEngine;
using Color = UnityEngine.Color;
using Transform = Box2D.NetStandard.Common.Transform;

namespace Box2D.WorldTests
{
    public class TestDebugDrawer : DebugDraw
    {
        public float scale = 1f; // world → unity scale
        
        public override void DrawTransform(in Transform xf)
        {
            Vector3 p = new Vector3(xf.p.X, xf.p.Y, 0f) * scale;
            float angle = xf.GetAngle();// depends on your Box2D port
            Quaternion rot = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(p, p + rot * UnityEngine.Vector3.right * 0.5f * scale);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(p, p + rot * Vector3.up * 0.5f * scale);
        }

        public override void DrawPoint(in System.Numerics.Vector2 position, float size, in Box2D.NetStandard.Dynamics.World.Color color)
        {
            Gizmos.color = new UnityEngine.Color(color.R, color.G, color.B, color.A);
            Vector3 p = new Vector3(position.X, position.Y, 0f) * scale;
            Gizmos.DrawSphere(p, size * scale * 0.1f);
        }

        public override void DrawPolygon(in Box2D.NetStandard.Common.Vec2[] vertices, int vertexCount, in Box2D.NetStandard.Dynamics.World.Color color)
        {
            Gizmos.color = new UnityEngine.Color(color.R, color.G, color.B, color.A);

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 p1 = new Vector3(vertices[i].X, vertices[i].Y, 0f) * scale;
                Vector3 p2 = new Vector3(vertices[(i + 1) % vertexCount].X, vertices[(i + 1) % vertexCount].Y, 0f) * scale;
                Gizmos.DrawLine(p1, p2);
            }
        }

        public override void DrawSolidPolygon(in Box2D.NetStandard.Common.Vec2[] vertices, int vertexCount, in Box2D.NetStandard.Dynamics.World.Color color)
        {
            UnityEngine.Color fill = new UnityEngine.Color(color.R, color.G, color.B, 0.25f);
            Gizmos.color = fill;

            Vector3 center = Vector3.zero;
            for (int i = 0; i < vertexCount; i++)
            {
                center += new Vector3(vertices[i].X, vertices[i].Y, 0f) * scale;
            }
            center /= vertexCount;

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 p1 = new Vector3(vertices[i].X, vertices[i].Y, 0f) * scale;
                Vector3 p2 = new Vector3(vertices[(i + 1) % vertexCount].X, vertices[(i + 1) % vertexCount].Y, 0f) * scale;
                Gizmos.DrawLine(p1, p2);
            }
        }

        public override void DrawCircle(in Box2D.NetStandard.Common.Vec2 center, float radius, in Box2D.NetStandard.Dynamics.World.Color color)
        {
            Gizmos.color = new UnityEngine.Color(color.R, color.G, color.B, color.A);

            Vector3 c = new Vector3(center.X, center.Y, 0f) * scale;
            float r = radius * scale;

            const int segments = 32;
            Vector3 prev = c + new Vector3(r, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float theta = (float)i / segments * Mathf.PI * 2f;
                Vector3 next = c + new Vector3(Mathf.Cos(theta) * r, Mathf.Sin(theta) * r, 0f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        public override void DrawSolidCircle(in Box2D.NetStandard.Common.Vec2 center, float radius, in Box2D.NetStandard.Common.Vec2 axis, in Box2D.NetStandard.Dynamics.World.Color color)
        {
            DrawCircle(center, radius, color);

            Gizmos.color = new UnityEngine.Color(color.R, color.G, color.B, color.A);

            Vector3 c = new Vector3(center.X, center.Y, 0f) * scale;
            float r = radius * scale;

            Vector3 axisDir = new Vector3(axis.X, axis.Y, 0f).normalized;
            Gizmos.DrawLine(c, c + axisDir * r);
        }

        public override void DrawSegment(in Box2D.NetStandard.Common.Vec2 p1, in Box2D.NetStandard.Common.Vec2 p2, in Box2D.NetStandard.Dynamics.World.Color color)
        {
            Gizmos.color = new UnityEngine.Color(color.R, color.G, color.B, color.A);

            Vector3 a = new Vector3(p1.X, p1.Y, 0f) * scale;
            Vector3 b = new Vector3(p2.X, p2.Y, 0f) * scale;

            Gizmos.DrawLine(a, b);
        }
    }
}