using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc
{
    public class GalacticPullStarEffectControllers : IGalacticPullStarEffectControllers, ILateUpdatable
    {
        private const float SPACE_BETWEEN_STARS = 20f;
        private const float DISTANCE_FROM_UI_CAMERA = 10f;
        private const float BOTTOM_PADDING_FRACTION = 0.12f;

        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly IWorldCameraController _worldCameraController;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly GalacticPullStarEffectPool _pool;
        private readonly List<GalacticPullStarEffectController> _controllers = new();
        private Transform _starsParent;

        public GalacticPullStarEffectControllers(GalacticPullStarEffectView prefab, DiContainer diContainer,
            PresentationGamePlayConfig gamePlayConfig, IStageCancellationTokenProvider stageCancellationTokenProvider,
            IWorldCameraController worldCameraController, IUpdateSubscriptionService updateSubscriptionService)
        {
            _gamePlayConfig = gamePlayConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _worldCameraController = worldCameraController;
            _updateSubscriptionService = updateSubscriptionService;
            _pool = new GalacticPullStarEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _starsParent = new GameObject("GalacticPullStarEffectsParent").transform;
            _starsParent.SetParent(_worldCameraController.CameraTransform, false);
            _starsParent.localPosition = new Vector3(0f, GetBottomLocalY(), DISTANCE_FROM_UI_CAMERA);
            _pool.InitPool();
            _updateSubscriptionService.RegisterLateUpdatable(this);
        }

        public void ManagedLateUpdate()
        {
            if (_starsParent == null)
            {
                return;
            }

            var localPosition = _starsParent.localPosition;
            localPosition.y = GetBottomLocalY();
            _starsParent.localPosition = localPosition;
        }

        private float GetBottomLocalY()
        {
            var orthographicSize = _worldCameraController.OrthographicSize;
            return -orthographicSize + orthographicSize * BOTTOM_PADDING_FRACTION;
        }

        public void ShowStar(ushort fieldId, ushort casterTeamId)
        {
            var outlineColor = _gamePlayConfig.ColorPerTeamId[casterTeamId];
            var controller = new GalacticPullStarEffectController(fieldId, _pool, _starsParent);
            controller.CreateView(outlineColor);
            _controllers.Add(controller);
            SlideInNewStarAndReflowExisting(controller);
        }

        public void HideStar(ushort fieldId)
        {
            var controller = GetStar(fieldId);
            _controllers.Remove(controller);
            controller.SlideOutAndDestroy(_stageCancellationTokenProvider.CancellationTokenSource).Forget();
            ReflowAll();
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.Destroy();
            }

            _controllers.Clear();
        }

        private void SlideInNewStarAndReflowExisting(GalacticPullStarEffectController newController)
        {
            var cancellationTokenSource = _stageCancellationTokenProvider.CancellationTokenSource;
            for (var i = 0; i < _controllers.Count; i++)
            {
                var controller = _controllers[i];
                var slotLocalX = GetSlotLocalX(i);
                if (controller == newController)
                {
                    controller.SlideIn(slotLocalX, cancellationTokenSource).Forget();
                }
                else
                {
                    controller.MoveToSlot(slotLocalX, cancellationTokenSource).Forget();
                }
            }
        }

        private void ReflowAll()
        {
            var cancellationTokenSource = _stageCancellationTokenProvider.CancellationTokenSource;
            for (var i = 0; i < _controllers.Count; i++)
            {
                _controllers[i].MoveToSlot(GetSlotLocalX(i), cancellationTokenSource).Forget();
            }
        }

        private float GetSlotLocalX(int index)
        {
            var centerOffset = (_controllers.Count - 1) * 0.5f;
            return (index - centerOffset) * SPACE_BETWEEN_STARS;
        }

        private GalacticPullStarEffectController GetStar(ushort fieldId)
        {
            var starController = _controllers.Find(controller => controller.FieldId == fieldId);
            if (starController == null)
            {
                LogService.LogError($"Tried to hide galactic pull star {fieldId} but it wasn't found!");
                return null;
            }

            return starController;
        }
    }
}
