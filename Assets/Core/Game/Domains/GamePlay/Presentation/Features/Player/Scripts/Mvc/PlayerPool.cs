using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public class PlayerPool : PrefabsPool<PlayerView>
    {
        protected override string ParentGameObjectName => "PlayersPool";

        public PlayerPool(PlayerView view, DiContainer diContainer) : base(
            new PoolData(4, 1), diContainer, view)
        {
        }
    }
}
