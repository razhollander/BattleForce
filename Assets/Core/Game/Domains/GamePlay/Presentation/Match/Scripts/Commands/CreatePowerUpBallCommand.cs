using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class CreatePowerUpBallCommand : BaseCommand, ICommandVoid
    {
        private IPowerUpBallControllers _powerUpBallControllers;
        private IMatchDataService _matchDataService;
        private IWorldCameraController _worldCameraController;

        private ushort _powerUpBallId;
        private Vector2 _position;

        public CreatePowerUpBallCommand SetPowerUpBallId(ushort powerUpBallId)
        {
            _powerUpBallId = powerUpBallId;
            return this;
        }

        public CreatePowerUpBallCommand SetPosition(Vector2 position)
        {
            _position = position;
            return this;
        }

        public override void ResolveDependencies()
        {
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
        }

        public void Execute()
        {
            _powerUpBallControllers.CreatePowerUpBall(_powerUpBallId, _position);

            if (!_matchDataService.IsInShowoffWinners)
            {
                _worldCameraController.AddFollowTarget(_powerUpBallControllers.GetPowerUpBallTransform(_powerUpBallId));
            }
        }
    }
}
