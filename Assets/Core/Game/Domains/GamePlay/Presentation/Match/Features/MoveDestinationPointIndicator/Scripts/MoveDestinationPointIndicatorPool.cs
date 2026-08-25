using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MoveDestinationPointIndicator.Scripts
{
    public class MoveDestinationPointIndicatorPool : PrefabsPool<MoveDestinationPointIndicatorView>
    {
        protected override string ParentGameObjectName => "MoveDestinationPointIndicatorParent";

        public MoveDestinationPointIndicatorPool(MoveDestinationPointIndicatorView view, DiContainer diContainer) : base(
            new PoolData(4, 2), diContainer, view)
        {
        }
    }
}
