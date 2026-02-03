using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc
{
    public class MatchBulletControllers : IMatchBulletControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly BulletPool _bulletPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchBulletController> _bulletControllers = new ();
        private Transform _bulletsParent;
        
        public MatchBulletControllers(IMatchDataService matchDataService, BulletView bulletViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _bulletPool = new BulletPool(bulletViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _bulletsParent = (new GameObject("BulletsParent")).transform;
            _bulletPool.InitPool();
        }

        public void CreateBullet(ushort bulletId, float bulletRadius, Vector2 position, Color color)
        {
            var bulletController = new MatchBulletController(bulletId, _matchDataService, _bulletPool, _bulletsParent);
            bulletController.CreateBulletView(position, bulletRadius, color);
            _bulletControllers.Add(bulletController);
        }

        public void UpdateBulletsTransform()
        {
            foreach (var bulletController in _bulletControllers)
            {
                bulletController.InterpolatePosition(_gamePlayConfig.InterpolationFactor);
            }
        }
        
        private MatchBulletController GetBullet(ushort bulletId)
        {
            return _bulletControllers.Find(x => x.BulletId == bulletId);
        }

        public void DestroyBullet(ushort bulletId)
        {
            var bulletController= GetBullet(bulletId);
            bulletController.Destroy();
            _bulletControllers.Remove(bulletController);
        }

        public void DestroyAll()
        {
            foreach (var bulletController in _bulletControllers)
            {
                bulletController.Destroy();
            }
            _bulletControllers.Clear();
        }
    }
}