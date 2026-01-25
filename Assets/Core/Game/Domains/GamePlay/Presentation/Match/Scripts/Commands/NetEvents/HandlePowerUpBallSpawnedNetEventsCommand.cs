using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePowerUpBallSpawnedNetEventsCommand  : BaseCommand, ICommandVoid
    {
        private IPowerUpBallControllers _powerUpBallControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var powerUpBallsSpawnEvents = _cachedPresentationEventsService.PowerUpBallSpawnedNetEvents;
            if (powerUpBallsSpawnEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var powerUpBallsSpawnEvent in powerUpBallsSpawnEvents)
            {
                var powerUpBallId = powerUpBallsSpawnEvent.PowerUpBallId;
                _powerUpBallControllers.CreatePowerUpBall(powerUpBallId, powerUpBallsSpawnEvent.Position.ToUnityVector2());
            }
            
            powerUpBallsSpawnEvents.Clear();
        }
    }
}