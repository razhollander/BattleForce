using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect
{
    public class DashPulseGustEffectPool : PrefabsPool<DashPulseGustEffectView>
    {
        protected override string ParentGameObjectName => "DashPulseGustEffectPool";

        public DashPulseGustEffectPool(DashPulseGustEffectView view, DiContainer diContainer) : base(
            new PoolData(6, 1), diContainer, view)
        {
        }
    }
}
