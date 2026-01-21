using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Mvc
{
    public class PowerUpBallControllers : IPowerUpBallControllers
    {
        private readonly PowerUpBallView _powerUpBallViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IMatchDataService _matchDataService;
        private readonly List<PowerUpBallController> _controllers = new List<PowerUpBallController>();
        private GameObject _parent;

        public PowerUpBallControllers(PowerUpBallView powerUpBallViewPrefab, PresentationGamePlayConfig gamePlayConfig, IMatchDataService matchDataService)
        {
            _powerUpBallViewPrefab = powerUpBallViewPrefab;
            _gamePlayConfig = gamePlayConfig;
            _matchDataService = matchDataService;
        }
        
        public void InitEntryPoint()
        {
            _parent = new GameObject("PowerUpBallParent");
        }
        
        public void CreatePowerUpBall(ushort powerUpBallId, Vector2 position)
        {
            var controller = new PowerUpBallController(powerUpBallId, _matchDataService);
            controller.CreateView(_powerUpBallViewPrefab, _parent.transform, position);
            _controllers.Add(controller);
        }

        public Vector2 GetPowerUpBallPosition(ushort powerUpBallId)
        {
            return GetController(powerUpBallId).GetPosition();
        }

        public void DestroyPowerUpBall(ushort cardId)
        {
            var cardController = GetController(cardId);
            cardController.DestroyView();
            _controllers.Remove(cardController);
        }

        public void UpdatePowerUpBallsTransform()
        {
            foreach (var controller in _controllers)
            {
                controller.InterpolatePosition(_gamePlayConfig.InterpolationFactor);
            }
        }

        private PowerUpBallController GetController(ushort powerUpBallId)
        {
            return _controllers.Find(x => x.PowerUpBallId == powerUpBallId);
        }
    }
}