using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardController
    {
        private TalentCardView _talentCardView;
        private readonly TalentCardsConfig _talentCardsConfig;
        private readonly TalentCard _talentCard;

        public TalentCardController(TalentCard talentCard, TalentCardsConfig talentCardsConfig)
        {
            _talentCard = talentCard;
            _talentCardsConfig = talentCardsConfig;
        }

        public void CreateView(TalentCardView talentCardViewPrefab, Transform parent)
        {
            _talentCardView = Object.Instantiate(talentCardViewPrefab, parent);
            _talentCardView.transform.position = _talentCard.Position.ToUnityVector3();

            if (_talentCardsConfig.TalentSprites.TryGetValue(_talentCard.TalentType, out var sprite))
            {
                _talentCardView.SetSprite(sprite);
            }
        }
    }
}
