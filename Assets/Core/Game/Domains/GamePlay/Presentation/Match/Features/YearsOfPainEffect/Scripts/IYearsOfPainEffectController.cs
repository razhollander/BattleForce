using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public interface IYearsOfPainEffectController
    {
        void InitEntryPoint();
        void PlayFieldEffect(Transform parentTransform, Vector2 direction);
        void PlayHitEffect(Vector2 position);
    }
}
