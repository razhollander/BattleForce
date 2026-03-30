using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect
{
    public interface IDashPulseGustEffectController
    {
        void InitEntryPoint();
        void PlayEffect(Vector2 position, Vector2 direction);
    }
}
