using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public class BulletController
    {
        private BulletView _bulletView;

        private readonly IMatchDataService _matchDataService;

        private readonly PresentationGamePlayConfig _gamePlayConfig;

        public readonly ushort BulletId;

         public BulletController(ushort bulletId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig)
         {
             _matchDataService = matchDataService;
             _gamePlayConfig = gamePlayConfig;
             BulletId = bulletId;
         }

         public void CreateBulletView(BulletView bulletViewPrefab, System.Numerics.Vector2 position, float radius, Transform parent, Color color)
        {
            _bulletView = Object.Instantiate(bulletViewPrefab, parent);
            _bulletView.name = "Bullet_" + BulletId;
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
            Object.Destroy(_bulletView.gameObject);
        }
    }
}