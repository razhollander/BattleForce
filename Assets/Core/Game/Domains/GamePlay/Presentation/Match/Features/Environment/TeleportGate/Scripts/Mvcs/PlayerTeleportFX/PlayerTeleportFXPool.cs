using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportFX
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
