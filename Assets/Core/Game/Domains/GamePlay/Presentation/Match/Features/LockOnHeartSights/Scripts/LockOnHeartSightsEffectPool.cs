using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnHeartSightsEffectPool : PrefabsPool<LockOnHeartSightEffectView>
    {
        protected override string ParentGameObjectName => "LockOnHeartSightsEffectParent";

        public LockOnHeartSightsEffectPool(LockOnHeartSightEffectView view, DiContainer diContainer) : base(
            new PoolData(10, 5), diContainer, view)
        {
        }
    }
}
