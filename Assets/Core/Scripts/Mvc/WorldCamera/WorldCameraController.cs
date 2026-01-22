using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public class WorldCameraController : IWorldCameraController
    {
        private readonly WorldCameraView _worldCameraView;

        public WorldCameraController(WorldCameraView worldCameraView)
        {
            _worldCameraView = worldCameraView;
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
    }
}
