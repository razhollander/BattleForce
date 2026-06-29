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
        private const float SPACE_BETWEEN_STARS = 2f;
        private const float DISTANCE_FROM_UI_CAMERA = 10f;
        private const float BOTTOM_PADDING_FRACTION = 0.12f;
        // Leaves headroom between stars so each star's renderers never collide with another star's order.
        private const int SORTING_ORDER_PER_STAR = 10;

        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly GalacticStarsVisualData _starsVisualData;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly IWorldCameraController _worldCameraController;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly GalacticPullStarEffectPool _pool;
        private readonly List<GalacticPullStarEffectController> _controllers = new();
        private Transform _starsParent;
        private int _nextVisualDataIndex;
        private int _nextSortingOrder;

        public GalacticPullStarEffectControllers(GalacticPullStarEffectView prefab, GalacticStarsVisualData starsVisualData,
            DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig,
            IStageCancellationTokenProvider stageCancellationTokenProvider,
            IWorldCameraController worldCameraController, IUpdateSubscriptionService updateSubscriptionService)
        {
            _gamePlayConfig = gamePlayConfig;
            _starsVisualData = starsVisualData;
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
            var visualData = GetNextVisualData();
            var controller = new GalacticPullStarEffectController(fieldId, _pool, _starsParent);
            controller.CreateView(outlineColor, visualData);
            controller.SetSortingOrder(_nextSortingOrder);
            _nextSortingOrder += SORTING_ORDER_PER_STAR;
            _controllers.Add(controller);
            ScaleInNewStarAndReflowExisting(controller);
        }

        public void HideStar(ushort fieldId)
        {
            var controller = GetStar(fieldId);
            if (controller == null)
            {
                return;
            }

            _controllers.Remove(controller);
            controller.SlideOutAndDestroyAsync(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
            ReflowAll();
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.Destroy();
            }

            _controllers.Clear();
            _nextSortingOrder = 0;
        }

        private void ScaleInNewStarAndReflowExisting(GalacticPullStarEffectController newController)
        {
            var cancellationToken = _stageCancellationTokenProvider.CancellationTokenSource.Token;
            for (var i = 0; i < _controllers.Count; i++)
            {
                var controller = _controllers[i];
                var slotLocalY = GetSlotLocalY(i);
                if (controller == newController)
                {
                    controller.ScaleInAsync(slotLocalY, cancellationToken).Forget();
                }
                else
                {
                    controller.MoveToSlotAsync(slotLocalY, cancellationToken).Forget();
                }
            }
        }

        private void ReflowAll()
        {
            var cancellationToken = _stageCancellationTokenProvider.CancellationTokenSource.Token;
            for (var i = 0; i < _controllers.Count; i++)
            {
                _controllers[i].MoveToSlotAsync(GetSlotLocalY(i), cancellationToken).Forget();
            }
        }

        // Stars stack vertically: the most recently added star sits at the base (localY 0)
        // and each older star is pushed one step further up.
        private float GetSlotLocalY(int index)
        {
            var slotsFromBottom = _controllers.Count - 1 - index;
            return slotsFromBottom * SPACE_BETWEEN_STARS;
        }

        private GalacticStarVisualData GetNextVisualData()
        {
            var visualData = _starsVisualData.Get(_nextVisualDataIndex);
            _nextVisualDataIndex = (_nextVisualDataIndex + 1) % _starsVisualData.Count;
            return visualData;
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
