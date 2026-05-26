using UnityEngine;

namespace Core.Scripts.Mvc.WorldCamera
{
    public interface IWorldCameraController
    {
        void AddFollowTarget(Transform target);
        void RemoveFollowTarget(Transform target);
        void ClearTargets();
        void ShakeCamera(float intensity, float durationInSeconds);

        void InitEntryPoint();

        void InitExitPoint();
        // void StopFollowTarget();
        // void StartFollowTarget(Transform targetTransform);
        // Awaitable DoLockOnTargetAnimation(Transform targetTransform, CancellationTokenSource cancellationTokenSource);
        Vector3 ScreenToWorldPoint(Vector3 position);
    }
}