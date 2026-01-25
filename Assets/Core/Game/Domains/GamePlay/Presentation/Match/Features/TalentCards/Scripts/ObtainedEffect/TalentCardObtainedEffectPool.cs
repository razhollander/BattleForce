using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect
{
    public class TalentCardObtainedEffectPool : PrefabsPool<TalentCardObtainedEffectView>
    {
        protected override string ParentGameObjectName => "TalentCardObtainedEffect";

        public TalentCardObtainedEffectPool(TalentCardObtainedEffectView view, DiContainer diContainer) : base(
            new PoolData(3, 1), diContainer, view)
        {
        }
    }
}