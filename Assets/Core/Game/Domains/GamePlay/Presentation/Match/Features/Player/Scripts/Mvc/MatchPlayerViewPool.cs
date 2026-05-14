using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerViewPool : PrefabsPool<MatchPlayerView>
    {
        protected override string ParentGameObjectName => "MatchPlayersPool";

        public MatchPlayerViewPool(MatchPlayerView view, DiContainer diContainer) : base(
            new PoolData(4, 1), diContainer, view)
        {
        }
    }
}
