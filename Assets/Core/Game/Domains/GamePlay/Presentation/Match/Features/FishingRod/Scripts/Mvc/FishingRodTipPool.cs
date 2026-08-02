using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc
{
    public class FishingRodTipPool : PrefabsPool<FishingRodTipView>
    {
        protected override string ParentGameObjectName => "Fishing Rod Tips Pool";

        public FishingRodTipPool(FishingRodTipView prefab, DiContainer diContainer) : base(new PoolData(3, 1), diContainer, prefab)
        {
        }
    }
}
