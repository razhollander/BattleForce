using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts
{
    public interface IMagneticPullEffectController
    {
        void InitEntryPoint();
        void PlayFieldEffect(Vector2 position, Vector2 rotation, float Radius);
        void PlayHitEffect(Vector2 casterPosition, Vector2 enemyPosition);
    }
}