using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardObtainedEffectPool : PrefabsPool<TalentCardObtainedEffectView>
    {
        protected override string ParentGameObjectName => "TalentCardObtainedEffect";

        public TalentCardObtainedEffectPool(TalentCardObtainedEffectView view, DiContainer diContainer) : base(
            new PoolData(3, 1), diContainer, view)
        {
        }
    }
}