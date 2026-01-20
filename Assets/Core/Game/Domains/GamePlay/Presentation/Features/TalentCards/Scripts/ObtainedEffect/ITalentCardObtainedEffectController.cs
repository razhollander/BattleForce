using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public interface ITalentCardObtainedEffectController
    {
        void PlayEffect(Vector2 from, Vector2 to);
        void InitEntryPoint();
    }
}