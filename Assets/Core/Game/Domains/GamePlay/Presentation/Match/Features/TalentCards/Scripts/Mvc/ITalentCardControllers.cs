using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc
{
    public interface ITalentCardControllers
    {
        void CreateTalentCard(ushort cardId);
        void DisplayTalentCardTakeDamaged(ushort cardId);
        Vector2 GetTalentCardPosition(ushort cardId);
        void DestroyTalentCard(ushort cardId);
        void DestroyAll();
        void InitEntryPoint();
    }
}
