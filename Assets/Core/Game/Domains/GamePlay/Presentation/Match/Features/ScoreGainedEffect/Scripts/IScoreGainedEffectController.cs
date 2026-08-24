using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGainedEffect.Scripts
{
    public interface IScoreGainedEffectController
    {
        void PlayEffect(byte gainedScore, Vector2 position, Color? outlineAndUnderlineColor = null);
        void InitEntryPoint();
    }
}
