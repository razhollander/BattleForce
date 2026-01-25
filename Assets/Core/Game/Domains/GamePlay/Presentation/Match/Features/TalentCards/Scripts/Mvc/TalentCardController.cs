using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc
{
    public class TalentCardController
    {
        private TalentCardView _talentCardView;
        private readonly TalentCardsConfig _talentCardsConfig;
        private readonly TalentCardPool _talentCardPool;
        private readonly Transform _parent;
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;

        public ushort TalentCardId { get; set; }

        public TalentCardController(ushort talentCardId, IMatchDataService matchDataService, TalentCardsConfig talentCardsConfig, TalentCardPool talentCardPool, Transform parent)
        {
            _matchDataService = matchDataService;
            _talentCardsConfig = talentCardsConfig;
            _talentCardPool = talentCardPool;
            _parent = parent;
            TalentCardId = talentCardId;
        }

        public void CreateView()
        {
            _talentCardView = _talentCardPool.Spawn();
            _talentCardView.transform.SetParent(_parent);
            var talentCardModel = _matchDataService.GetTalentCard(TalentCardId);
            _talentCardView.transform.position = talentCardModel.Position;

            if (_talentCardsConfig.TalentSprites.TryGetValue(talentCardModel.TalentType, out var sprite))
            {
                _talentCardView.SetTalentSprite(sprite);
            }

            _talentCardView.SwapToFullHealthSprite();
        }

        public void DestroyView()
        {
            _talentCardView.Despawn();
        }

        public void SetDamaged()
        {
            _talentCardView.SwapToDamagedSprite();
        }

        public Vector2 GetPosition()
        {
            return _talentCardView.transform.position;
        }
    }
}
