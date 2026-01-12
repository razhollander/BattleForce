using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public interface ITalentCardObtainedEffectController
    {
        void PlayEffect(Vector2 from, Vector2 to);
    }

    public class TalentCardObtainedEffectController : PrefabsPool<TalentCardObtainedEffectView>, ITalentCardObtainedEffectController
    {
        public TalentCardObtainedEffectController(TalentCardsConfig config, DiContainer diContainer) : base(
            new PoolData(10, 5), diContainer, config.TalentCardObtainedEffectPrefab)
        {
        }

        public void PlayEffect(Vector2 from, Vector2 to)
        {
            var view = Spawn();
            view.Init(Despawn);
            view.Play(from, to, 0.5f);
        }
    }
}
