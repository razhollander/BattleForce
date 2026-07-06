using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public interface ILockOnTargetShootEffectController
    {
        void InitEntryPoint();
        void Play(Vector2 startPosition, Vector2 targetPosition);
    }
}
