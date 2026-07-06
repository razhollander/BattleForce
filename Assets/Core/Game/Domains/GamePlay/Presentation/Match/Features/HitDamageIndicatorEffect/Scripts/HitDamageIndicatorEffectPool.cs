using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts
{
    public class HitDamageIndicatorEffectPool : PrefabsPool<HitDamageIndicatorEffectView>
    {
        protected override string ParentGameObjectName => "HitDamageIndicatorEffectParent";

        public HitDamageIndicatorEffectPool(HitDamageIndicatorEffectView view, DiContainer diContainer) : base(
            new PoolData(5, 2), diContainer, view)
        {
        }
    }
}
