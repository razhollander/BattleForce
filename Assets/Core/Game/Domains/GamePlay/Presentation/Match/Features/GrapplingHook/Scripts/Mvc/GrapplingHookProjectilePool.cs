using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc
{
    public class GrapplingHookProjectilePool
    {
        private readonly GrapplingHookProjectileView _prefab;
        private readonly DiContainer _diContainer;
        private IPrefabsPool<GrapplingHookProjectileView> _pool;

        public GrapplingHookProjectilePool(GrapplingHookProjectileView prefab, DiContainer diContainer)
        {
            _prefab = prefab;
            _diContainer = diContainer;
        }

        public void InitPool()
        {
            _pool = new PrefabsPool<GrapplingHookProjectileView>(_prefab, _diContainer);
            _pool.Init(5);
        }

        public GrapplingHookProjectileView Spawn()
        {
            return _pool.Spawn();
        }
    }
}
