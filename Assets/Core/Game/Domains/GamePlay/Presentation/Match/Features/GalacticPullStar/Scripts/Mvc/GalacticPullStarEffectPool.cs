using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc
{
    public class GalacticPullStarEffectPool : PrefabsPool<GalacticPullStarEffectView>
    {
        protected override string ParentGameObjectName => "GalacticPullStarEffectsPool";

        public GalacticPullStarEffectPool(GalacticPullStarEffectView view, DiContainer diContainer) : base(
            new PoolData(5, 2), diContainer, view)
        {
        }
    }
}
