using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public interface IWorldCameraController
    {
        void StopFollowTarget();
        void StartFollowTarget(Transform targetTransform);
        Awaitable DoLockOnTargetAnimation(Transform targetTransform, CancellationTokenSource cancellationTokenSource);
    }
}