using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public class YearsOfPainViewPool : PrefabsPool<YearsOfPainView>
    {
        protected override string ParentGameObjectName => "YearsOfPainViewPool";

        public YearsOfPainViewPool(PoolData poolData, YearsOfPainView prefab, DiContainer container) : base(poolData, container, prefab)
        {
        }
    }
}
