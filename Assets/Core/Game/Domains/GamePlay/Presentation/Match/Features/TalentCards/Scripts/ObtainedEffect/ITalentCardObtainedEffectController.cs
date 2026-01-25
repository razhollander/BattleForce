using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect
{
    public interface ITalentCardObtainedEffectController
    {
        void PlayEffect(Vector2 from, Vector2 to);
        void InitEntryPoint();
    }
}