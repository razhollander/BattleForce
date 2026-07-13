using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePowerUpBallSpawnedNetEventsCommand  : BaseCommand, ICommandVoid
    {
        private IPowerUpBallControllers _powerUpBallControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
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
            _audioService.PlayAudio(AudioClipType.PowerUpBallSpawned);
            powerUpBallsSpawnEvents.Clear();
        }
    }
}