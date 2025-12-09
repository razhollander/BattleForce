using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public class BulletControllers : IBulletControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly BulletView _bulletViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<BulletController> _bulletControllers = new ();
        private GameObject _bulletsParent;
        
        public BulletControllers(IMatchDataService matchDataService, BulletView bulletViewPrefab, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _bulletViewPrefab = bulletViewPrefab;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _bulletsParent = new GameObject("BulletsParent");
        }

        public void CreateBullet(int bulletId)
        {
            var bulletController = new BulletController(bulletId, _matchDataService, _gamePlayConfig);
            bulletController.CreateBulletView(_bulletViewPrefab, _bulletsParent.transform);
            _bulletControllers.Add(bulletController);
        }

        public void UpdateBulletsTransform()
        {
            foreach (var bulletController in _bulletControllers)
            {
                bulletController.InterpolatePosition(_gamePlayConfig.InterpolationFactor);
            }
        }
    }
}