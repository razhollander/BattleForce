using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.PlayerTeleportFX
{
    public class PlayerTeleportFXPool : PrefabsPool<PlayerTeleportFXView>
    {
        protected override string ParentGameObjectName => "PlayerTeleportFX";

        public PlayerTeleportFXPool(PlayerTeleportFXView view, DiContainer diContainer) : base(
            new PoolData(5, 1), diContainer, view)
        {
        }
    }
}
