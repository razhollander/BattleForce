using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public class BulletPool : PrefabsPool<BulletView>
    {
        protected override string ParentGameObjectName => "BulletsPool";

        public BulletPool(BulletView view, DiContainer diContainer) : base(
            new PoolData(50, 10), diContainer, view)
        {
        }
    }
}
