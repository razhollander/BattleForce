using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GainedBoltEffect.Scripts
{
    public class GainBoltEffectPool: PrefabsPool<GainBoltEffectView>
    {
        protected override string ParentGameObjectName => "GainBoltEffectParent";

        public GainBoltEffectPool(GainBoltEffectView view, DiContainer diContainer) : base(
            new PoolData(3, 1), diContainer, view)
        {
        }
    }
}