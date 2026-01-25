using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc
{
    public interface IMatchBulletControllers
    {
        void InitEntryPoint();
        void CreateBullet(ushort bulletId, float bulletRadius, Vector2 position, Color color);
        void UpdateBulletsTransform();
        void DestroyBullet(ushort bulletId);
    }
}