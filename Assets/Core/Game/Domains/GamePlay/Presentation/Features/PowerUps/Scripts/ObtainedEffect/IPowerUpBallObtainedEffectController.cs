using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.ObtainedEffect
{
    public interface IPowerUpBallObtainedEffectController
    {
        Awaitable PlayEffect(Vector2 from, Vector2 to);
        void InitEntryPoint();
    }
}