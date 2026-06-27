using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Mvc.UICamera;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc
{
    public class GalacticPullStarEffectControllers : IGalacticPullStarEffectControllers
    {
        private const float SPACE_BETWEEN_STARS = 10f;
        private const float DISTANCE_FROM_UI_CAMERA = 10f;

        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly IUICameraController _uiCameraController;
        private readonly GalacticPullStarEffectPool _pool;
        private readonly List<GalacticPullStarEffectController> _controllers = new();
        private Transform _starsParent;

        public GalacticPullStarEffectControllers(GalacticPullStarEffectView prefab, DiContainer diContainer,
            PresentationGamePlayConfig gamePlayConfig, IStageCancellationTokenProvider stageCancellationTokenProvider,
            IUICameraController uiCameraController)
        {
            _gamePlayConfig = gamePlayConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _uiCameraController = uiCameraController;
            _pool = new GalacticPullStarEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _starsParent = new GameObject("GalacticPullStarEffectsParent").transform;
            _starsParent.SetParent(_uiCameraController.UICamera.transform, false);
            _starsParent.localPosition = new Vector3(0f, 0f, DISTANCE_FROM_UI_CAMERA);
            _pool.InitPool();
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
            if (controller == null)
            {
                LogService.LogError($"Tried to hide galactic pull star {fieldId} but it wasn't found!");
                return;
            }

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
            var offsetFromNewest = _controllers.Count - 1 - index;
            return -offsetFromNewest * SPACE_BETWEEN_STARS;
        }

        private GalacticPullStarEffectController GetStar(ushort fieldId)
        {
            return _controllers.Find(controller => controller.FieldId == fieldId);
        }
    }
}
