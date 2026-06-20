using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public class LockOnTargetShootEffectPool : PrefabsPool<LockOnTargetShootEffectView>
    {
        protected override string ParentGameObjectName => "LockOnTargetShootEffectParent";

        public LockOnTargetShootEffectPool(LockOnTargetShootEffectView view, DiContainer diContainer) : base(
            new PoolData(5, 3), diContainer, view)
        {
        }
    }
}
