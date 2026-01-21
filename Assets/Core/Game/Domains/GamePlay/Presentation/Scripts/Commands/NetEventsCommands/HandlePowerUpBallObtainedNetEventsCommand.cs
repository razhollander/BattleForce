using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class HandlePowerUpBallObtainedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IPowerUpBallObtainedEffectController _powerUpBallObtainedEffectController;
        private IPlayerControllers _playerControllers;
        private IPowerUpBallControllers _powerUpBallControllers;

        public override void ResolveDependencies()
        {
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _powerUpBallObtainedEffectController = _diContainer.Resolve<IPowerUpBallObtainedEffectController>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var powerUpBallObtainedNetEvents = _cachedPresentationEventsService.PowerUpBallObtainedNetEvents;
            if (powerUpBallObtainedNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var powerUpBallObtainedEvent in powerUpBallObtainedNetEvents)
            {
                ushort powerUpBallId = powerUpBallObtainedEvent.Id;
                var powerUpBallPosition = _powerUpBallControllers.GetPowerUpBallPosition(powerUpBallId);
                var playerPosition = _playerControllers.GetPlayerPosition(powerUpBallObtainedEvent.ObtainedByPlayerId);
                _powerUpBallObtainedEffectController.PlayEffect(powerUpBallPosition, playerPosition);
                _powerUpBallControllers.DestroyPowerUpBall(powerUpBallId);
            }
            
            powerUpBallObtainedNetEvents.Clear();
        }
    }
}