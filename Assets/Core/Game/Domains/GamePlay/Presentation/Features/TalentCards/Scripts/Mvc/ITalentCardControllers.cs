using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public interface ITalentCardControllers
    {
        void CreateTalentCard(ushort cardId);
        void DisplayTalentCardTakeDamaged(ushort cardId);
        Vector2 GetTalentCardPosition(ushort cardId);
        void DestroyTalentCard(ushort cardId);
        void InitEntryPoint();
    }
}
