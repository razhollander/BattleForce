using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.LevelEnvironment.Scripts
{
    // Authoring marker for an EnvironmentGateTrap. Place one in a layout, give it a wall shape, the two poses its wall
    // swings between and the polygons that sense the players, then bake it into the EnvironmentConfig via
    // EnvironmentGenerator.RefreshGateTraps. Every position is authored in world space, which for a trap riding a
    // rotating wheel is read as the wheel's local space, exactly like the wheel's own walls.
    public class GateTrapSpawnPoint : MonoBehaviour
    {
        public ushort Id;

        // Shares the layout's wall id space - it must not collide with an authored wall or a wheel wall.
        public ushort WallId;

        public PolygonPath2D WallShape;
        public List<PolygonPath2D> AreaPolygons = new();

        // The wall's pose in each state. The Z euler of each transform is the wall's rotation there.
        public Transform OpenPose;
        public Transform ClosedPose;

        // Wall-local point the wall turns around while it swings between the poses.
        public Transform LocalRotationPivot;

        public float MovementSpeed = 12f;
        public float SecondsStayClosed = 2f;
        public float SecondsStayOpen = 3f;

        public bool IsAttachedToRotationWheel;
        public ushort AttachToRotationWheelId;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawPose(OpenPose, Color.green);
            DrawPose(ClosedPose, Color.red);

            if (OpenPose != null && ClosedPose != null)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(OpenPose.position, ClosedPose.position);
            }
        }

        private void DrawPose(Transform pose, Color color)
        {
            if (pose == null || WallShape == null || WallShape.Points.Count < 2)
            {
                return;
            }

            Gizmos.matrix = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
            Gizmos.color = color;
            var points = WallShape.Points;

            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
            }
        }
#endif
    }
}
