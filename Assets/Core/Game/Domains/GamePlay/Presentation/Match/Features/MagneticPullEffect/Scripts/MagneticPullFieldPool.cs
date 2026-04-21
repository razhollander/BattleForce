using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts
{
    public class MagneticPullFieldPool : PrefabsPool<MagneticPullFieldView>
    {
        protected override string ParentGameObjectName => "MagneticPullFieldParent";

        public MagneticPullFieldPool(MagneticPullFieldView view, DiContainer diContainer) : base(
            new PoolData(3, 1), diContainer, view)
        {
        }
    }
}