using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Vector2 = System.Numerics.Vector2;

namespace Core.Scripts.Utils
{
    public static class DebugDrawUtils
    {
        [Conditional("DEBUG_DRAW_ENABLED")]
        public static void DrawRotatedRect(Vector2 centerNum, Vector2 sizeNum, float angleRadians)
        {
            // 1. Convert System.Numerics to Unity types
            Vector3 center = new Vector3(centerNum.X, centerNum.Y, 0);
            float halfWidth = sizeNum.X / 2f;
            float halfHeight = sizeNum.Y / 2f;

            // 2. Define the 4 corners in local space (centered at 0,0)
            Vector3 topLeft = new Vector3(-halfWidth, halfHeight, 0);
            Vector3 topRight = new Vector3(halfWidth, halfHeight, 0);
            Vector3 bottomLeft = new Vector3(-halfWidth, -halfHeight, 0);
            Vector3 bottomRight = new Vector3(halfWidth, -halfHeight, 0);

            // 3. Create a rotation quaternion from your radians
            // Note: We multiply by Rad2Deg because Unity rotations use degrees
            Quaternion rotation = Quaternion.Euler(0, 0, angleRadians * Mathf.Rad2Deg);

            // 4. Rotate corners and shift to world center
            Vector3 p1 = center + (rotation * topLeft);
            Vector3 p2 = center + (rotation * topRight);
            Vector3 p3 = center + (rotation * bottomRight);
            Vector3 p4 = center + (rotation * bottomLeft);

            var duration = 1f;
            // 5. Draw the lines in the Scene View
            Debug.DrawLine(p1, p2, Color.green, duration); // Top
            Debug.DrawLine(p2, p3, Color.green, duration); // Right
            Debug.DrawLine(p3, p4, Color.green, duration); // Bottom
            Debug.DrawLine(p4, p1, Color.green, duration); // Left
        }

        public static void DrawPolygon(Vector2 centerNum, Vector2[] localVertices)
        {
            if (localVertices == null || localVertices.Length < 2) return;

            Vector3 center = new Vector3(centerNum.X, centerNum.Y, 0);
            int count = localVertices.Length;
            float duration = 1f;
            Color drawColor = Color.green;

            for (int i = 0; i < count; i++)
            {
                // 1. Get current and next vertex in local space
                var currentLocal = localVertices[i];
                var nextLocal = localVertices[(i + 1) % count]; // Loop back to start

                // 2. Convert and offset to world space
                Vector3 p1 = center + new Vector3(currentLocal.X, currentLocal.Y, 0);
                Vector3 p2 = center + new Vector3(nextLocal.X, nextLocal.Y, 0);

                // 3. Draw the segment
                Debug.DrawLine(p1, p2, drawColor, duration);
            }
        }
    }
}