using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public interface IBulletControllers
    {
        void InitEntryPoint();
        void CreateBullet(ushort bulletId, ushort belongToPlayerId, float bulletRadius, Vector2 position);
        void UpdateBulletsTransform();
        void DestroyBullet(ushort bulletId);
    }
}