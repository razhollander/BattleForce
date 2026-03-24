using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts
{
    public class KOProjectilePool : PrefabsPool<KOProjectileView>
    {
        protected override string ParentGameObjectName => "KO Projectiles Pool";

        public KOProjectilePool(KOProjectileView prefab, DiContainer diContainer) : base(new PoolData(3, 1), diContainer, prefab)
        {
        }
    }
}
