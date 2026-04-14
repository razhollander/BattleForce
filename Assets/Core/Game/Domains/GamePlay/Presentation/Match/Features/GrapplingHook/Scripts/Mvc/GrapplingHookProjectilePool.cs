using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc
{
    public class GrapplingHookProjectilePool : PrefabsPool<GrapplingHookProjectileView>
    {
        protected override string ParentGameObjectName => "Grappling Hook Projectiles Pool";

        public GrapplingHookProjectilePool(GrapplingHookProjectileView prefab, DiContainer diContainer) : base(new PoolData(3, 1), diContainer, prefab)
        {
        }
    }
}
