using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc
{
    public interface IGrapplingHookProjectilesControllers
    {
        void InitEntryPoint();
        void CreateGrapplingHookProjectile(ushort hookId, ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 casterPosition, bool isAttached);
        void InterpolateGrapplingHookTransform(ushort hookId, Vector2 position, Quaternion rotation, Vector2 casterPosition);
        void UpdateOnHit(ushort hookId);
        void DestroyGrapplingHookProjectile(ushort hookId);
        void DestroyAll();
    }
}
