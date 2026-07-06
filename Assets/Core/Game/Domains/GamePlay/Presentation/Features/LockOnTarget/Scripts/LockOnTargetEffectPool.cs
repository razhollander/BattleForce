using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public class LockOnTargetEffectPool : PrefabsPool<LockOnTargetEffectView>
    {
        protected override string ParentGameObjectName => "LockOnTargetEffectParent";

        public LockOnTargetEffectPool(LockOnTargetEffectView view, DiContainer diContainer) : base(
            new PoolData(5, 3), diContainer, view)
        {
        }
    }
}
