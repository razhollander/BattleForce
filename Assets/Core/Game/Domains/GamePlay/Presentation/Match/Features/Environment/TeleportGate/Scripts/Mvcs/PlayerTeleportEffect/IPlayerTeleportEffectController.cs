using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect
{
    public interface IPlayerTeleportEffectController
    {
        void InitEntryPoint();
        void PlayEffect(Vector2 position);
    }
}