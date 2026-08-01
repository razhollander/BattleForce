using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePowerUpBallSpawnedNetEventsCommand  : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;
        private ICommandFactory _commandFactory;
        private CreatePowerUpBallCommand _createPowerUpBallCommand;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _createPowerUpBallCommand = _commandFactory.CreateCommandVoid<CreatePowerUpBallCommand>();
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
                _createPowerUpBallCommand.SetPowerUpBallId(powerUpBallId).SetPosition(powerUpBallsSpawnEvent.Position.ToUnityVector2()).Execute();
            }
            _audioService.PlayAudio(AudioClipType.PowerUpBallSpawned);
            powerUpBallsSpawnEvents.Clear();
        }
    }
}