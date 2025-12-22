namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public interface IBulletControllers
    {
        void InitEntryPoint();
        void CreateBullet(ushort bulletId);
        void UpdateBulletsTransform();
        void DestroyBullet(ushort bulletId);
    }
}