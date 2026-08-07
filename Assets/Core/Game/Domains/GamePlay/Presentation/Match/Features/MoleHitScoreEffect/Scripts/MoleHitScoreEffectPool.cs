using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MoleHitScoreEffect.Scripts
{
    public class MoleHitScoreEffectPool : PrefabsPool<MoleHitScoreEffectView>
    {
        protected override string ParentGameObjectName => "MoleHitScoreEffectParent";

        public MoleHitScoreEffectPool(MoleHitScoreEffectView view, DiContainer diContainer) : base(
            new PoolData(5, 2), diContainer, view)
        {
        }
    }
}
