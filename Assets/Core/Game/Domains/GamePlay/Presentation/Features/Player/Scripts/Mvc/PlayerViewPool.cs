using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerViewPool : PrefabsPool<PlayerView>
    {
        protected override string ParentGameObjectName => "PlayersPool";

        public PlayerViewPool(PlayerView view, DiContainer diContainer) : base(
            new PoolData(4, 1), diContainer, view)
        {
        }
    }
}
