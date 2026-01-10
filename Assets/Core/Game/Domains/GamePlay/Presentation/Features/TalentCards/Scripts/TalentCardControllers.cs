using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
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
        }

        public void CreateTalentCards(FixedUnorderedList<TalentCard> talentCards)
        {
            if (_parent == null)
            {
                _parent = new GameObject("TalentCards");
            }

            foreach (var talentCard in talentCards.AsSpan())
            {
                var controller = new TalentCardController(talentCard, _gamePlayConfig.TalentCards);
                controller.CreateView(_talentCardViewPrefab, _parent.transform);
                _controllers.Add(controller);
            }
        }
    }
}
