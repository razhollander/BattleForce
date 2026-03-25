using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public interface IKOProjectilesControllers
    {
        void InitEntryPoint();
        void CreateKOProjectile(ushort koProjectileId, Vector2 position, Vector2 rotation, Vector2 coilStartPoint, float size);
        void InterpulateKOProjectileTransform(ushort koProjectileId, Vector2 position, Quaternion rotation, Vector2 coilSpringStartPosition);
        void DestroyKOProjectile(ushort koProjectileId);
        void DestroyAll();
    }
}