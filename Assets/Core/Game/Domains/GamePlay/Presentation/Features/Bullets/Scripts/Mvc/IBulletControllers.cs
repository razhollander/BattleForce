namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public interface IBulletControllers
    {
        void InitEntryPoint();
        void CreateBullet(int bulletId);
        void UpdateBulletsTransform();
    }
}