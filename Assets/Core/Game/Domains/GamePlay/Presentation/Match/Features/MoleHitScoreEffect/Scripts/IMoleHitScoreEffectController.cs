using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MoleHitScoreEffect.Scripts
{
    public interface IMoleHitScoreEffectController
    {
        void PlayEffect(byte gainedScore, Vector2 position);
        void InitEntryPoint();
    }
}
