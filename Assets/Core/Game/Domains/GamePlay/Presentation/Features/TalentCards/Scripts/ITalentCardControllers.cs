using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public interface ITalentCardControllers
    {
        void CreateTalentCards(FixedUnorderedList<TalentCard> talentCards);
    }
}
