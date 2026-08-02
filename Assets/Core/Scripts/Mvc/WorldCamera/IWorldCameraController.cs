using System.Threading;
using UnityEngine;

namespace Core.Scripts.Mvc.WorldCamera
{
    public interface IWorldCameraController
    {
        Transform CameraTransform { get; }
        float OrthographicSize { get; }
        void AddFollowTarget(Transform target);
        void RemoveFollowTarget(Transform target);
        void ClearTargets();
        void ShakeCamera(float intensity, float durationInSeconds);
        void MultiplyOthographicSize(float multiplier);
        public void SetisDampingEnabled(bool isEnabled);
        void SetWorldBoundaries(Vector2 topLeft, Vector2 bottomRight);
        Awaitable LerpOrthographicSizeMultiplier(float targetMultiplier, float durationSeconds, CancellationToken cancellationToken);
        void InitEntryPoint();

        void InitExitPoint();
        Vector3 ScreenToWorldPoint(Vector3 position);
    }
}