using UnityEngine;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc
{
    public class GrapplingHookProjectileController
    {
        private const string HookProjectileName = "GrapplingHookProjectile_";
        private readonly ushort _hookId;
        private readonly GrapplingHookProjectilePool _grapplingHookProjectilePool;
        private readonly Transform _parent;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private GrapplingHookProjectileView _view;
        private string _cachedName;

        public ushort CasterPlayerId { get; private set; }

        public GrapplingHookProjectileController(ushort hookId, ushort casterPlayerId, GrapplingHookProjectilePool grapplingHookProjectilePool, Transform parent, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _hookId = hookId;
            CasterPlayerId = casterPlayerId;
            _grapplingHookProjectilePool = grapplingHookProjectilePool;
            _parent = parent;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void CreateView(Vector2 position, Quaternion rotation, Vector2 casterPosition, bool isAttached)
        {
            _view = _grapplingHookProjectilePool.Spawn();
            _view.name =  HookProjectileName + _hookId;
            _view.transform.SetParent(_parent);
            _view.SetIsAttached(isAttached);
            _view.Setup(position, rotation, casterPosition, _sharedGamePlayConfig.GrapplingHookProjectileMaxDistance);
        }

        public void InterpolateTransform(Vector2 position, Quaternion rotation, Vector2 casterPosition, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(_view.Transform.position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(_view.Transform.rotation, rotation, decay, Time.deltaTime);
            _view.SetTransform(lerpedPosition, lerpedRotation, casterPosition);
        }

        public void UpdateOnHit()
        {
            _view.UpdateOnHit();
        }

        public void Destroy()
        {
            _view.Despawn();
        }
    }
}
