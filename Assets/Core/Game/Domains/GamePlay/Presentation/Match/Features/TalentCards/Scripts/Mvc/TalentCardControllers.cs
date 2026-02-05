using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc
{
    public class TalentCardControllers : ITalentCardControllers
    {
        private readonly TalentCardPool _talentCardPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IMatchDataService _matchDataService;
        private readonly List<TalentCardController> _controllers = new List<TalentCardController>();
        private Transform _parent;

        public TalentCardControllers(TalentCardView talentCardViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig, IMatchDataService matchDataService)
        {
            _talentCardPool = new TalentCardPool(talentCardViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
            _matchDataService = matchDataService;
        }

        public void InitEntryPoint()
        {
            _parent = (new GameObject("TalentCardsParent")).transform;
            _talentCardPool.InitPool();
        }
        
        public void CreateTalentCard(ushort cardId)
        {
            var controller = new TalentCardController(cardId, _matchDataService, _gamePlayConfig.TalentCards, _talentCardPool, _parent);
            controller.CreateView();
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
            cardController.DestroyView();
            _controllers.Remove(cardController);
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.DestroyView();
            }
            _controllers.Clear();
        }

        private TalentCardController GetController(ushort cardId)
        {
            return _controllers.Find(x => x.TalentCardId == cardId);
        }
    }
}
