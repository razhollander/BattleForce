using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardControllers : ITalentCardControllers
    {
        private readonly TalentCardView _talentCardViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<TalentCardController> _controllers = new List<TalentCardController>();
        private GameObject _parent;

        public TalentCardControllers(TalentCardView talentCardViewPrefab, PresentationGamePlayConfig gamePlayConfig)
        {
            _talentCardViewPrefab = talentCardViewPrefab;
            _gamePlayConfig = gamePlayConfig;
            _parent = new GameObject("TalentCardsParent");
        }

        public void CreateTalentCards(FixedUnorderedList<TalentCardS2C> talentCards)
        {
            foreach (var talentCard in talentCards.AsSpan())
            {
                var controller = new TalentCardController(talentCard, _gamePlayConfig.TalentCards);
                controller.CreateView(_talentCardViewPrefab, _parent.transform);
                _controllers.Add(controller);
            }
        }
    }
}
