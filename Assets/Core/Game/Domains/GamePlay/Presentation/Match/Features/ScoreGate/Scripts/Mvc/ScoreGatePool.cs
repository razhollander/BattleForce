using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc
{
    public class ScoreGatePool : PrefabsPool<ScoreGateView>
    {
        protected override string ParentGameObjectName => "ScoreGatesPool";

        public ScoreGatePool(ScoreGateView prefab, DiContainer diContainer) : base(new PoolData(4, 1), diContainer, prefab)
        {
        }
    }
}
