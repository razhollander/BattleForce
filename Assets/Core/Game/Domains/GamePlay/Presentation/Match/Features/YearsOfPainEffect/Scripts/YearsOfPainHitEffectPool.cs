using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public class YearsOfPainHitEffectPool : PrefabsPool<YearsOfPainHitEffectView>
    {
        public YearsOfPainHitEffectPool(YearsOfPainHitEffectView prefab, DiContainer container) : base(prefab, container)
        {
        }
    }
}
