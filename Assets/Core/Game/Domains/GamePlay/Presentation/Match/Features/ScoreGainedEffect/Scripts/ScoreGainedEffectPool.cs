using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGainedEffect.Scripts
{
    public class ScoreGainedEffectPool : PrefabsPool<ScoreGainedEffectView>
    {
        protected override string ParentGameObjectName => "ScoreGainedEffectParent";

        public ScoreGainedEffectPool(ScoreGainedEffectView view, DiContainer diContainer) : base(
            new PoolData(5, 2), diContainer, view)
        {
        }
    }
}
