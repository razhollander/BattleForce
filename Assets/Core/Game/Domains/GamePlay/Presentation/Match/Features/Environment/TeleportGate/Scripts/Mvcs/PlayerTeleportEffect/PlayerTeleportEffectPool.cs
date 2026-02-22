using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect
{
    public class PlayerTeleportEffectPool : PrefabsPool<PlayerTeleportEffectView>
    {
        protected override string ParentGameObjectName => "PlayerTeleportEffectParent";

        public PlayerTeleportEffectPool(PlayerTeleportEffectView view, DiContainer diContainer) : base(
            new PoolData(6, 1), diContainer, view)
        {
        }
    }
}
