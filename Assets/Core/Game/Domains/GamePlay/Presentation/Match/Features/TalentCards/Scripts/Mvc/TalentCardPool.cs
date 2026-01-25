using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc
{
    public class TalentCardPool : PrefabsPool<TalentCardView>
    {
        protected override string ParentGameObjectName => "TalentCardsPool";

        public TalentCardPool(TalentCardView view, DiContainer diContainer) : base(
            new PoolData(10, 2), diContainer, view)
        {
        }
    }
}
