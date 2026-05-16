using Core.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc
{
    public class BackgroundParallaxController : IUpdatable, IBackgroundParallaxController
    {
        private readonly BackgroundParallaxView _backgroundParallaxView;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IWorldCameraController _worldCameraController;

        public BackgroundParallaxController(BackgroundParallaxView backgroundParallaxView, IUpdateSubscriptionService updateSubscriptionService, IWorldCameraController worldCameraController)
        {
            _backgroundParallaxView = backgroundParallaxView;
            _updateSubscriptionService = updateSubscriptionService;
            _worldCameraController = worldCameraController;
        }

        public void InitEntryPoint()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
        }

        public void ManagedUpdate()
        {
            var screenCenterInWorldPosition = _worldCameraController.ScreenToWorldPoint(Vector3.zero);
            _backgroundParallaxView.MoveLayers(-screenCenterInWorldPosition);
        }

        public void InitExitPoint()
        {
            _updateSubscriptionService.UnregisterUpdatable(this);
        }
    }
}
