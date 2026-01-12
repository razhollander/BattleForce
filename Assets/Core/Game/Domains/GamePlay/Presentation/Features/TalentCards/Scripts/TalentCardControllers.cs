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

        public void DestroyTalentCard(ushort cardId)
        {
            for (var i = _controllers.Count - 1; i >= 0; i--)
            {
                if (_controllers[i].TalentCard.Id == cardId)
                {
                    _controllers[i].DestroyView();
                    _controllers.RemoveAt(i);
                    return;
                }
            }
        }

        public void SetTalentCardDamaged(ushort cardId)
        {
            foreach (var controller in _controllers)
            {
                if (controller.TalentCard.Id == cardId)
                {
                    controller.SetDamaged();
                    return;
                }
            }
        }

        public bool TryGetCardPosition(ushort cardId, out Vector2 position)
        {
            foreach (var controller in _controllers)
            {
                if (controller.TalentCard.Id == cardId)
                {
                    position = controller.GetPosition();
                    return true;
                }
            }
            position = Vector2.zero;
            return false;
        }
    }
}
