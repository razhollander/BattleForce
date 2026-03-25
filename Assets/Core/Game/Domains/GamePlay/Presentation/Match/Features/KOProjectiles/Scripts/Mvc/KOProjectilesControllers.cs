using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public class KOProjectilesControllers : IKOProjectilesControllers
    {
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IMatchDataService _matchDataService;
        private readonly KOProjectilePool _koProjectilePool;
        private readonly Dictionary<ushort, KOProjectileController> _controllers = new();
        private Transform _koProjectilesParent;

        public KOProjectilesControllers(KOProjectileView koProjectileViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
            _koProjectilePool = new KOProjectilePool(koProjectileViewPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _koProjectilesParent = (new GameObject("KOProjectilesParent")).transform;
            _koProjectilePool.InitPool();
        }

        public void CreateKOProjectile(ushort koProjectileId, Vector2 position, Vector2 rotation, Vector2 coilStartPoint, float size)
        {
            var koProjectileController = new KOProjectileController(koProjectileId, _koProjectilePool, _koProjectilesParent);
            koProjectileController.CreateKOPorjectileView(position, rotation.ToQuaternion(), coilStartPoint, size);
            _controllers.Add(koProjectileId, koProjectileController);
        }

        public void InterpulateKOProjectileTransform(ushort koProjectileId, Vector2 position, Quaternion rotation, Vector2 coilSpringStartPosition)
        {
            var koProjectileController = GetKOProjectile(koProjectileId);
            koProjectileController.InterpolateTransform(position, rotation, coilSpringStartPosition, _gamePlayConfig.ExponentialDecay);
        }
        
        public void DestroyKOProjectile(ushort koProjectileId)
        {
            var koProjectileController = GetKOProjectile(koProjectileId);
            koProjectileController.Destroy();
            _controllers.Remove(koProjectileId);
        }

        private KOProjectileController GetKOProjectile(ushort koProjectileId)
        {
            return _controllers[koProjectileId];
        }
        
        public void DestroyAll()
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Destroy();
            }
            
            _controllers.Clear();
        }
    }
}
