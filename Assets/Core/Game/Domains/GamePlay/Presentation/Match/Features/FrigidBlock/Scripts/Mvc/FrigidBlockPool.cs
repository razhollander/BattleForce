using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlockPool : PrefabsPool<FrigidBlockView>
    {
        protected override string ParentGameObjectName => "FrigidBlocksPool";

        public FrigidBlockPool(FrigidBlockView prefab, DiContainer diContainer) : base(new PoolData(3, 1), diContainer, prefab)
        {
        }
    }
}
