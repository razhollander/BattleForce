using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public readonly struct CameraTarget
    {
        public readonly Transform Transform;
        public readonly float Radius;

        public CameraTarget(Transform transform, float radius)
        {
            Transform = transform;
            Radius = radius;
        }
    }
}
