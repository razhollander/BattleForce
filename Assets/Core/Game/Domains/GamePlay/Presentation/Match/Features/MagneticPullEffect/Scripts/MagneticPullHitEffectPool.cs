using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts
{
    public class MagneticPullHitEffectPool : PrefabsPool<MagneticPullHitEffectView>
    {
        protected override string ParentGameObjectName => "MagneticPullHitEffectParent";

        public MagneticPullHitEffectPool(MagneticPullHitEffectView view, DiContainer diContainer) : base(
            new PoolData(3, 1), diContainer, view)
        {
        }
    }
}