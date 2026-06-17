using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WaterGunStream.Scripts.Mvc
{
    public class WaterGunStreamPool : PrefabsPool<WaterGunStreamView>
    {
        protected override string ParentGameObjectName => "Water Gun Stream Pool";

        public WaterGunStreamPool(WaterGunStreamView prefab, DiContainer diContainer) : base(new PoolData(8, 2), diContainer, prefab)
        {
        }
    }
}
