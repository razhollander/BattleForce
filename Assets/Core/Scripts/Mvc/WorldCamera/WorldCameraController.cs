using System.Threading;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace Core.Scripts.Mvc.WorldCamera
{
    public class WorldCameraController : IWorldCameraController, ILateUpdatable
    {
        private const float CameraTargetRadius = 5f;

        private readonly WorldCameraView _worldCameraView;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;

        public WorldCameraController(WorldCameraView worldCameraView, IUpdateSubscriptionService updateSubscriptionService)
        {
            _worldCameraView = worldCameraView;
            _updateSubscriptionService = updateSubscriptionService;
        }

        public Transform CameraTransform => _worldCameraView.Camera.transform;

        public float OrthographicSize => _worldCameraView.Camera.orthographicSize;

        public void InitEntryPoint()
        {
            _worldCameraView.Setup();
            _updateSubscriptionService.RegisterLateUpdatable(this);
        }

        public void InitExitPoint()
        {
            _updateSubscriptionService.UnregisterLateUpdatable(this);
            _worldCameraView.Cleanup();
        }

        public void MultiplyOthographicSize(float multiplier)
        {
            _worldCameraView.MultiplyOthographicSize(multiplier);
        }

        public void SetisDampingEnabled(bool isEnabled)
        {
            _worldCameraView.SetIsDampingEnabled(isEnabled);
        }

        public void SetWorldBoundaries(Vector2 topLeft, Vector2 bottomRight)
        {
            LogService.LogTopic($"Set camera world boundaries topLeft {topLeft} bottomRight {bottomRight}", LogTopicType.Camera);
            _worldCameraView.SetWorldBoundaries(topLeft, bottomRight);
        }
        
        public async Awaitable LerpOrthographicSizeMultiplier(float targetMultiplier, float durationSeconds, CancellationToken cancellationToken)
        {
            await _worldCameraView.LerpOrthographicSize(targetMultiplier, durationSeconds, cancellationToken);
        }
        
        public void AddFollowTarget(Transform target)
        {
            LogService.LogTopic($"Add camera target {target.gameObject.name}", LogTopicType.Camera);
            _worldCameraView.AddFollowTarget(target, CameraTargetRadius);
        }

        public void RemoveFollowTarget(Transform target)
        {
            LogService.LogTopic($"Remove camera target {target.gameObject.name}", LogTopicType.Camera);
            _worldCameraView.RemoveFollowTarget(target);
        }

        public void ClearTargets()
        {
            LogService.LogTopic("Clear all camera targets", LogTopicType.Camera);
            _worldCameraView.ClearTargets();
        }

        public void ShakeCamera(float intensity, float duration)
        {
            LogService.LogTopic($"Shake camera with intensity {intensity} for {duration} seconds", LogTopicType.Camera);
            _worldCameraView.ShakeCamera(intensity, duration);
        }

        public Vector3 ScreenToWorldPoint(Vector3 position)
        {
            return _worldCameraView.ScreenToWorldPoint(position);
        }

        public void ManagedLateUpdate()
        {
            _worldCameraView.UpdateFraming(Time.deltaTime);
            _worldCameraView.BaseCamera.orthographicSize = _worldCameraView.Camera.orthographicSize;
        }
    }
}
