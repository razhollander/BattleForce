using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GainedBoltEffect.Scripts
{
    public interface IGainBoltEffectController
    {
        void PlayEffect(int boltsAmount, Vector2 position, Transform parent);
        void InitEntryPoint();
    }
}
