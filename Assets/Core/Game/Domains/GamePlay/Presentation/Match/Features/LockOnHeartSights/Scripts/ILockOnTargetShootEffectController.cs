using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public interface ILockOnTargetShootEffectController
    {
        void InitEntryPoint();
        void Play(Vector2 casterHeadPosition, Vector2 targetHeartPosition);
    }
}
