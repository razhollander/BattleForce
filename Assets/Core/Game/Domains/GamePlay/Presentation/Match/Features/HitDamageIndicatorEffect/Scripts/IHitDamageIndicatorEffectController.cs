using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts
{
    public interface IHitDamageIndicatorEffectController
    {
        void PlayEffect(ushort damage, Vector2 position, Transform parent);
        void InitEntryPoint();
    }
}
