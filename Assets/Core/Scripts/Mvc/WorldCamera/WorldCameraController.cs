using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public class WorldCameraController : IWorldCameraController, IUpdatable
    {
        private readonly WorldCameraView _worldCameraView;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private Transform _followTarget;

        public WorldCameraController(WorldCameraView worldCameraView, IUpdateSubscriptionService updateSubscriptionService)
        {
            _worldCameraView = worldCameraView;
            _updateSubscriptionService = updateSubscriptionService;
        }
        
        public void StartFollowTarget(Transform targetTransform)
        {
            LogService.LogTopic($"Start follow target {targetTransform.gameObject.name}", LogTopicType.Camera );
            _followTarget = targetTransform;
            SetCameraRelativeToTarget(_followTarget);
            _updateSubscriptionService.RegisterUpdatable(this);
        }

        public async Awaitable DoLockOnTargetAnimation(Transform targetTransform, CancellationTokenSource cancellationTokenSource)
        {
            LogService.LogTopic($"Do lock on target animation {targetTransform.gameObject.name}", LogTopicType.Camera );
            SetCameraRelativeToTarget(targetTransform);
            await _worldCameraView.DoLockOnTargetAnimation(_worldCameraView.transform.position, _worldCameraView.transform.rotation.eulerAngles, cancellationTokenSource);
        }

        public void StopFollowTarget()
        {
            LogService.LogTopic("Stop follow target", LogTopicType.Camera );
            _updateSubscriptionService.UnregisterUpdatable(this);
            _followTarget = null;
        }

        public void ManagedUpdate()
        { 
            LerpCameraRelativeToTarget();
        }

        private void LerpCameraRelativeToTarget()
        {
            _worldCameraView.LerpPositionRelativeToTarget(_followTarget);
            _worldCameraView.LookAtTarget(_followTarget);
        }
        
        private void SetCameraRelativeToTarget(Transform target)
        {
            _worldCameraView.SetPositionRelativeToTarget(target);
            _worldCameraView.LookAtTarget(target);
        }
    }
}
