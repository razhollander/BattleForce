using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc
{
    public class ChickenEggPool : PrefabsPool<ChickenEggView>
    {
        protected override string ParentGameObjectName => "ChickenEggsPool";

        public ChickenEggPool(ChickenEggView view, DiContainer diContainer) : base(
            new PoolData(20, 5), diContainer, view)
        {
        }
    }
}
