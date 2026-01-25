using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc
{
    public class MatchBulletController
    {
        private BulletView _bulletView;

        private readonly IMatchDataService _matchDataService;

        private readonly BulletPool _bulletPool;

        public readonly ushort BulletId;
        private readonly Transform _bulletsParent;

        public MatchBulletController(ushort bulletId, IMatchDataService matchDataService, BulletPool bulletPool, Transform bulletsParent)
         {
             _bulletPool = bulletPool;
             _matchDataService = matchDataService;
             BulletId = bulletId;
             _bulletsParent = bulletsParent;
         }

         public void CreateBulletView(System.Numerics.Vector2 position, float radius, Color color)
        {
            _bulletView = _bulletPool.Spawn();
            _bulletView.name = "Bullet_" + BulletId;
            _bulletView.transform.SetParent(_bulletsParent);
            _bulletView.SetPosition(position.ToUnity());
            _bulletView.SetRadius(radius);
            _bulletView.SetColor(color);
        }

        public void InterpolatePosition(float interpolationFactor)
        {
            var bulletModel = _matchDataService.GetBullet(BulletId);
            var bulletPosition = bulletModel.Position.ToUnity();
            _bulletView.InterpolatePosition(bulletPosition, interpolationFactor);
        }

        public void Destroy()
        {
            _bulletView.Despawn();
        }
    }
}