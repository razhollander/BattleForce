using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc
{
    public class FishingRodTipControllers : IFishingRodTipControllers
    {
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly FishingRodTipPool _pool;
        private readonly Dictionary<ushort, FishingRodTipController> _controllers = new();
        private Transform _parentTransform;

        public FishingRodTipControllers(FishingRodTipView prefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
            _pool = new FishingRodTipPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _parentTransform = (new GameObject("FishingRodTipsParent")).transform;
            _pool.InitPool();
        }

        public void CreateFishingRodTip(ushort tipId, ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 casterPosition)
        {
            var controller = new FishingRodTipController(tipId, casterPlayerId, _pool, _parentTransform);
            controller.CreateView(position, rotation.ToQuaternion(), casterPosition);
            _controllers[tipId] = controller;
        }

        public void InterpolateFishingRodTipTransform(ushort tipId, Vector2 position, Quaternion rotation, Vector2 casterPosition)
        {
            if (_controllers.TryGetValue(tipId, out var controller))
            {
                controller.InterpolateTransform(position, rotation, casterPosition, _gamePlayConfig.ExponentialDecay);
            }
        }

        public void DestroyFishingRodTip(ushort tipId)
        {
            if (_controllers.TryGetValue(tipId, out var controller))
            {
                controller.Destroy();
                _controllers.Remove(tipId);
            }
            else
            {
                Debug.LogError($"FishingRodTipController with id {tipId} not found");
            }
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Destroy();
            }
            _controllers.Clear();
        }
    }
}
