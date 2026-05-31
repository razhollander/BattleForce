using System.Diagnostics;
using Core.Scripts.Extensions;
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
            Vector3 center = new Vector3(centerNum.X, centerNum.Y, 0);
            float halfWidth = sizeNum.X / 2f;
            float halfHeight = sizeNum.Y / 2f;
            
            Vector3 topLeft = new Vector3(-halfWidth, halfHeight, 0);
            Vector3 topRight = new Vector3(halfWidth, halfHeight, 0);
            Vector3 bottomLeft = new Vector3(-halfWidth, -halfHeight, 0);
            Vector3 bottomRight = new Vector3(halfWidth, -halfHeight, 0);
            
            Quaternion rotation = Quaternion.Euler(0, 0, angleRadians * Mathf.Rad2Deg);
            
            Vector3 p1 = center + (rotation * topLeft);
            Vector3 p2 = center + (rotation * topRight);
            Vector3 p3 = center + (rotation * bottomRight);
            Vector3 p4 = center + (rotation * bottomLeft);

            var duration = 1f;
           
            Debug.DrawLine(p1, p2, Color.green, duration); // Top
            Debug.DrawLine(p2, p3, Color.green, duration); // Right
            Debug.DrawLine(p3, p4, Color.green, duration); // Bottom
            Debug.DrawLine(p4, p1, Color.green, duration); // Left
        }

        [Conditional("DEBUG_DRAW_ENABLED")]
        public static void DrawPolygon(Vector2 centerNum, Vector2[] localVertices)
        {
            if (localVertices == null || localVertices.Length < 2) return;

            Vector3 center = new Vector3(centerNum.X, centerNum.Y, 0);
            int count = localVertices.Length;
            float duration = 0.02f;
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
        
        [Conditional("DEBUG_DRAW_ENABLED")]
        public static void DrawLine(Vector3 point1, Vector3 point2, Color color)
        {
            float duration = 0.02f;
            Debug.DrawLine(point1, point2, color, duration);
        }
        
            /// <summary>
            /// Draws a 2D debug arc in the Scene view.
            /// </summary>
            /// <param name="position">The center point of the circle.</param>
            /// <param name="direction">The forward direction of the arc in 2D space.</param>
            /// <param name="radius">The radius of the arc.</param>
            /// <param name="halfArcAngleDegrees">Half of the total total arc angle.</param>
            /// <param name="segments">How many lines to use to draw the arc (higher is smoother).</param>
            /// <param name="color">Color of the debug lines.</param>
            [Conditional("DEBUG_DRAW_ENABLED")]
            public static void DrawArc2D(Vector2 position, Vector2 direction, float radius, float halfArcAngleDegrees, int segments = 20, Color color = default)
            {
                if (color == default) color = Color.white;
        
                var dir = direction.Normalize();
                var drawDurationInSeconds = 0.02f;
                // Find the base angle of the direction vector in degrees
                var baseAngle = Mathf.Atan2(dir.Y, dir.X) * Mathf.Rad2Deg;
        
                var startAngle = baseAngle - halfArcAngleDegrees;
                var angleStep = (halfArcAngleDegrees * 2f) / segments;

                // Calculate the starting point of the arc
                var startRad = startAngle * Mathf.Deg2Rad;
                var currentPoint = position + new Vector2(Mathf.Cos(startRad), Mathf.Sin(startRad)) * radius;
                Debug.DrawLine(currentPoint.ToUnityVector2(), position.ToUnityVector2(), color, drawDurationInSeconds);

                // Loop through and draw each segment
                for (int i = 1; i <= segments; i++)
                {
                    var currentRad = (startAngle + (angleStep * i)) * Mathf.Deg2Rad;
                    var nextPoint = position + new Vector2(Mathf.Cos(currentRad), Mathf.Sin(currentRad)) * radius;
            
                    Debug.DrawLine(currentPoint.ToUnityVector2(), nextPoint.ToUnityVector2(), color, drawDurationInSeconds);
                    currentPoint = nextPoint;
                }
                
                Debug.DrawLine(currentPoint.ToUnityVector2(), position.ToUnityVector2(), color, drawDurationInSeconds);
            }
    }
}