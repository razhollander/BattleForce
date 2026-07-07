using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts
{
    public interface IHeadbuttHitEffectController
    {
        void InitEntryPoint();
        void PlayEffect(Vector2 position);
    }
}
