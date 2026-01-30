using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets
{
    public class MatchMakingBulletControllers : IMatchMakingBulletControllers
    {
        private readonly IMatchMakingDataService _matchDataService;
        private readonly BulletPool _bulletPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchMakingBulletController> _bulletControllers = new ();
        private Transform _bulletsParent;
        
        public MatchMakingBulletControllers(IMatchMakingDataService matchDataService, BulletView bulletViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
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

        public void CreateBullet(ushort bulletId, float bulletRadius, Vector2 position, ushort belongToPlayerId)
        {
            var player = _matchDataService.GetPlayer(belongToPlayerId);
            if (player == null)
            {
                return;
            }

            var bulletColor = _gamePlayConfig.ColorPerTeamId[player.TeamId];
            var bulletController = new MatchMakingBulletController(bulletId, _matchDataService, _bulletPool, _bulletsParent);
            bulletController.CreateBulletView(position, bulletRadius, bulletColor);
            _bulletControllers.Add(bulletController);
        }

        public void UpdateBulletsTransform()
        {
            foreach (var bulletController in _bulletControllers)
            {
                bulletController.InterpolatePosition(_gamePlayConfig.InterpolationFactor);
            }
        }
        
        private MatchMakingBulletController GetBullet(ushort bulletId)
        {
            return _bulletControllers.Find(x => x.BulletId == bulletId);
        }

        public void DestroyBullet(ushort bulletId)
        {
            var bulletController= GetBullet(bulletId);
            bulletController.Destroy();
            _bulletControllers.Remove(bulletController);
        }
    }
}