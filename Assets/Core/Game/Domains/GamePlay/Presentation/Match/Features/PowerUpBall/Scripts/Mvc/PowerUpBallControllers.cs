using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Mvc.WorldCamera;
using UnityEngine;
using Zenject;
using Core.Game.Domains.GamePlay.Presentation.Scripts.DataService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc
{
    public class PowerUpBallControllers : IPowerUpBallControllers
    {
        private readonly PowerUpBallPool _pool;
        private readonly IInterpolationDecayService _interpolationDecayService;
        private readonly IMatchDataService _matchDataService;
        private readonly IWorldCameraController _worldCameraController;
        private readonly List<PowerUpBallController> _controllers = new List<PowerUpBallController>();
        private Transform _parent;

        public PowerUpBallControllers(PowerUpBallView powerUpBallViewPrefab, DiContainer diContainer, IInterpolationDecayService interpolationDecayService, IMatchDataService matchDataService, IWorldCameraController worldCameraController)
        {
            _pool = new PowerUpBallPool(powerUpBallViewPrefab, diContainer);
            _interpolationDecayService = interpolationDecayService;
            _matchDataService = matchDataService;
            _worldCameraController = worldCameraController;
        }
        
        public void InitEntryPoint()
        {
            _parent = (new GameObject("PowerUpBallParent")).transform;
            _pool.InitPool();
        }
        
        public void CreatePowerUpBall(ushort powerUpBallId, Vector2 position)
        {
            var controller = new PowerUpBallController(powerUpBallId, _matchDataService, _pool, _parent);
            controller.CreateView(position);
            _controllers.Add(controller);
        }

        public Vector2 GetPowerUpBallPosition(ushort powerUpBallId)
        {
            return GetController(powerUpBallId).GetPosition();
        }

        public Transform GetPowerUpBallTransform(ushort powerUpBallId)
        {
            return GetController(powerUpBallId).GetTransform();
        }

        public void DestroyPowerUpBall(ushort cardId)
        {
            var cardController = GetController(cardId);
            _worldCameraController.RemoveFollowTarget(cardController.GetTransform());
            cardController.DestroyView();
            _controllers.Remove(cardController);
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                _worldCameraController.RemoveFollowTarget(controller.GetTransform());
                controller.DestroyView();
            }
            _controllers.Clear();
        }

        public void UpdatePowerUpBallsTransform()
        {
            foreach (var controller in _controllers)
            {
                controller.InterpolatePosition(_interpolationDecayService.CurrentDecay);
            }
        }

        private PowerUpBallController GetController(ushort powerUpBallId)
        {
            return _controllers.Find(x => x.PowerUpBallId == powerUpBallId);
        }
    }
}