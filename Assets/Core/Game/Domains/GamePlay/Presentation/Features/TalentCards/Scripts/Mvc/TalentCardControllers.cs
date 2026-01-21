using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardControllers : ITalentCardControllers
    {
        private readonly TalentCardPool _talentCardPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IMatchDataService _matchDataService;
        private readonly List<TalentCardController> _controllers = new List<TalentCardController>();
        private readonly GameObject _parent;

        public TalentCardControllers(TalentCardPool talentCardPool, PresentationGamePlayConfig gamePlayConfig, IMatchDataService matchDataService)
        {
            _talentCardPool = talentCardPool;
            _gamePlayConfig = gamePlayConfig;
            _matchDataService = matchDataService;
            _parent = new GameObject("TalentCardsParent");
        }

        public void CreateTalentCard(ushort cardId)
        {
            var controller = new TalentCardController(cardId, _matchDataService, _gamePlayConfig.TalentCards);
            controller.CreateView(_talentCardPool, _parent.transform);
            _controllers.Add(controller);
        }

        public void DisplayTalentCardTakeDamaged(ushort cardId)
        {
            GetController(cardId).SetDamaged();
        }

        public Vector2 GetTalentCardPosition(ushort cardId)
        {
            return GetController(cardId).GetPosition();
        }

        public void DestroyTalentCard(ushort cardId)
        {
            var cardController = GetController(cardId);
            cardController.DestroyView(_talentCardPool);
            _controllers.Remove(cardController);
        }

        private TalentCardController GetController(ushort cardId)
        {
            return _controllers.Find(x => x.TalentCardId == cardId);
        }
    }
}
