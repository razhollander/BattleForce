using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardController
    {
        private TalentCardView _talentCardView;
        private readonly TalentCardsConfig _talentCardsConfig;
        private readonly TalentCardS2C _talentCard;

        public TalentCardController(TalentCardS2C talentCard, TalentCardsConfig talentCardsConfig)
        {
            _talentCard = talentCard;
            _talentCardsConfig = talentCardsConfig;
        }

        public void CreateView(TalentCardView talentCardViewPrefab, Transform parent)
        {
            _talentCardView = Object.Instantiate(talentCardViewPrefab, parent);
            _talentCardView.transform.position = _talentCard.Position.ToUnityVector2();

            if (_talentCardsConfig.TalentSprites.TryGetValue(_talentCard.TalentType, out var sprite))
            {
                _talentCardView.SetSprite(sprite);
            }
        }
    }
}
