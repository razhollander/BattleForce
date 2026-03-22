using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts
{
    public class SwapFieldPool : PrefabsPool<SwapFieldView>
    {
        protected override string ParentGameObjectName => "SwapFieldsPool";

        public SwapFieldPool(SwapFieldView view, DiContainer diContainer) : base(
            new PoolData(2, 1), diContainer, view)
        {
        }
    }
}
