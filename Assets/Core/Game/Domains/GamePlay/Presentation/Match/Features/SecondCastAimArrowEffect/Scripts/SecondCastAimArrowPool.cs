using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts
{
    public class SecondCastAimArrowPool : PrefabsPool<SecondCastAimArrowView>
    {
        protected override string ParentGameObjectName => "Second Cast Aim Arrows Pool";

        public SecondCastAimArrowPool(SecondCastAimArrowView prefab, DiContainer diContainer) : base(new PoolData(3, 1), diContainer, prefab)
        {
        }
    }
}
