using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public class MolePool : PrefabsPool<MoleView>
    {
        protected override string ParentGameObjectName => "MolesPool";

        public MolePool(MoleView view, DiContainer diContainer) : base(
            new PoolData(8, 4), diContainer, view)
        {
        }
    }
}
