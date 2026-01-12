using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public interface ITalentCardControllers
    {
        void CreateTalentCards(FixedUnorderedList<TalentCardS2C> talentCards);
        void DestroyTalentCard(ushort cardId);
        void SetTalentCardDamaged(ushort cardId);
        bool TryGetCardPosition(ushort cardId, out Vector2 position);
    }
}
