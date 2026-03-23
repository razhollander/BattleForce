using UnityEngine;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public class KOProjectileController
    {
        private readonly ushort _koProjectileId;
        private readonly KOProjectilePool _koProjectilePool;
        private readonly Transform _parent;
        private KOProjectileView _view;
        
        public KOProjectileController(ushort koProjectileId, KOProjectilePool koProjectilePool, Transform parent)
        {
            _koProjectileId = koProjectileId;
            _koProjectilePool = koProjectilePool;
            _parent = parent;
        }

        public void CreateKOPorjectileView(Vector2 position, Quaternion rotation, Vector2 coilSpringStartPosition, float size)
        {
            _view = _koProjectilePool.Spawn();
            _view.name = "KOProjectile_" + _koProjectileId;
            _view.transform.SetParent(_parent);
            _view.Setup(position, rotation, coilSpringStartPosition, size);
        }
        
        public void InterpolateTransform(Vector2 position, Quaternion rotation, Vector2 coilSpringStartPosition, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(_view.Transform.position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(_view.Transform.rotation, rotation, decay, Time.deltaTime);
            _view.SetTransform(lerpedPosition, lerpedRotation, coilSpringStartPosition);
        }
        
        public void Destroy()
        {
            _view.Despawn();
        }
    }
}
