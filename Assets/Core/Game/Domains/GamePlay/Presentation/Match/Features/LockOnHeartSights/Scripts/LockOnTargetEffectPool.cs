using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetEffectPool : PrefabsPool<LockOnTargetEffectView>
    {
        protected override string ParentGameObjectName => "LockOnTargetEffectParent";

        public LockOnTargetEffectPool(LockOnTargetEffectView view, DiContainer diContainer) : base(
            new PoolData(10, 5), diContainer, view)
        {
        }
    }
}
