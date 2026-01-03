using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public interface IBulletControllers
    {
        void InitEntryPoint();
        void CreateBullet(ushort bulletId, ushort belongToPlayerId, float bulletRadius, Vector2 position, Color color);
        void UpdateBulletsTransform();
        void DestroyBullet(ushort bulletId);
    }
}