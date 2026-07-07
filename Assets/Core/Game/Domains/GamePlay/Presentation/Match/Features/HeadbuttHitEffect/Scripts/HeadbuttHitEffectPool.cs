using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts
{
    public class HeadbuttHitEffectPool : PrefabsPool<HeadbuttHitEffectView>
    {
        protected override string ParentGameObjectName => "HeadbuttHitEffectParent";

        public HeadbuttHitEffectPool(HeadbuttHitEffectView view, DiContainer diContainer) : base(
            new PoolData(6, 2), diContainer, view)
        {
        }
    }
}
