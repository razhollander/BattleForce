using System.Collections.Generic;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public class WorldCameraController : IWorldCameraController
    {
        private readonly WorldCameraView _worldCameraView;
        private readonly IStateMachineService _stateMachineService;

        public WorldCameraController(WorldCameraView worldCameraView, IStateMachineService stateMachineService)
        {
            _worldCameraView = worldCameraView;
            _stateMachineService = stateMachineService;
        }

        public void AddTarget(Transform target)
        {
            LogService.LogTopic($"Add camera target {target.gameObject.name}", LogTopicType.Camera);
            _worldCameraView.AddTarget(target, 1f, 5f);
        }

        public void RemoveTarget(Transform target)
        {
            LogService.LogTopic($"Remove camera target {target.gameObject.name}", LogTopicType.Camera);
            _worldCameraView.RemoveTarget(target);
        }

        public void ClearTargets()
        {
            LogService.LogTopic("Clear all camera targets", LogTopicType.Camera);
            _worldCameraView.ClearTargets();
        }

        public void ShakeCamera(float intensity, float duration)
        {
            LogService.LogTopic($"Shake camera with intensity {intensity} for {duration} seconds", LogTopicType.Camera);
            _worldCameraView.ShakeCamera(intensity, duration, _stateMachineService.CurrentState().CancellationTokenSource).Forget();
        }
    }
}
