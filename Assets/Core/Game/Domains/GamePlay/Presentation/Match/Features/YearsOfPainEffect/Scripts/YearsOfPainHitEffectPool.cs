using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public class YearsOfPainHitEffectPool : PrefabsPool<YearsOfPainHitEffectView>
    {
        protected override string ParentGameObjectName => "YearsOfPainHitEffectPool";

        public YearsOfPainHitEffectPool(PoolData data, YearsOfPainHitEffectView prefab, DiContainer container) : base(data, container, prefab)
        {
        }
    }
}
