using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;
using Core.Scripts.Extensions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.DataService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc
{
    public class GrapplingHookProjectilesControllers : IGrapplingHookProjectilesControllers
    {
        private readonly IInterpolationDecayService _interpolationDecayService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly GrapplingHookProjectilePool _pool;
        private readonly Dictionary<ushort, GrapplingHookProjectileController> _controllers = new();
        private Transform _parentTransform;

        public GrapplingHookProjectilesControllers(GrapplingHookProjectileView prefab, DiContainer diContainer, IInterpolationDecayService interpolationDecayService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _interpolationDecayService = interpolationDecayService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _pool = new GrapplingHookProjectilePool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _parentTransform = (new GameObject("GrapplingHookProjectilesParent")).transform;
            _pool.InitPool();
        }

        public void CreateGrapplingHookProjectile(ushort hookId, ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 casterPosition, bool isHookAttached)
        {
            var controller = new GrapplingHookProjectileController(hookId, casterPlayerId, _pool, _parentTransform, _sharedGamePlayConfig);
            controller.CreateView(position, rotation.ToQuaternion(), casterPosition, isHookAttached);
            _controllers.Add(hookId, controller);
        }

        public void InterpolateGrapplingHookTransform(ushort hookId, Vector2 position, Quaternion rotation, Vector2 casterPosition)
        {
            if (_controllers.TryGetValue(hookId, out var controller))
            {
                controller.InterpolateTransform(position, rotation, casterPosition, _interpolationDecayService.CurrentDecay);
            }
        }

        public void UpdateOnHit(ushort hookId)
        {
            if (_controllers.TryGetValue(hookId, out var controller))
            {
                controller.UpdateOnHit();
            }
        }

        public void DestroyGrapplingHookProjectile(ushort hookId)
        {
            if (_controllers.TryGetValue(hookId, out var controller))
            {
                controller.Destroy();
                _controllers.Remove(hookId);
            }
            else
            {
                Debug.LogError($"GrapplingHookProjectileController with id {hookId} not found");
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
