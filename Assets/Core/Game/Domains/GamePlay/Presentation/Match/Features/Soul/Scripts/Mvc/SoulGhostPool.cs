using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc
{
    public class SoulGhostPool : PrefabsPool<SoulGhostView>
    {
        protected override string ParentGameObjectName => "Soul Ghosts Pool";

        public SoulGhostPool(SoulGhostView prefab, DiContainer diContainer) : base(new PoolData(3, 1), diContainer, prefab)
        {
        }
    }
}
