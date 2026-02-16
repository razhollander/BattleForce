using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core.Scripts.Utils
{
    public static class DebugDrawUtils
    {
        [Conditional("PHYSICS_DEBUG_DRAW_ENABLED")]
        public static void DrawRotatedRect(System.Numerics.Vector2 centerNum, System.Numerics.Vector2 sizeNum, float angleRadians)
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
    }
}