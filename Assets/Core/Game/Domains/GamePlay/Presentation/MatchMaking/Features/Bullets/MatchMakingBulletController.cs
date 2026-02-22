using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets
{
    public class MatchMakingBulletController
    {
        private BulletView _bulletView;

        private readonly IMatchMakingDataService _matchDataService;

        private readonly BulletPool _bulletPool;

        public readonly ushort BulletId;
        private readonly Transform _bulletsParent;

        public MatchMakingBulletController(ushort bulletId, IMatchMakingDataService matchDataService, BulletPool bulletPool, Transform bulletsParent)
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
            _bulletView.SetPosition(position.ToUnityVector2());
            _bulletView.SetRadius(radius);
            _bulletView.SetColor(color);
        }

        public void InterpolatePosition(float decay)
        {
            var bulletModel = _matchDataService.GetBullet(BulletId);
            var bulletPosition = bulletModel.Position.ToUnityVector2();
            _bulletView.InterpolatePosition(bulletPosition, decay);
        }

        public void Destroy()
        {
            _bulletView.Despawn();
        }
    }
}