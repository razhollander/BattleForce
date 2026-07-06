using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePowerUpBallObtainedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IPowerUpBallObtainedEffectController _powerUpBallObtainedEffectController;
        private IMatchPlayerControllers _playerControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _powerUpBallObtainedEffectController = _diContainer.Resolve<IPowerUpBallObtainedEffectController>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
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
                _audioService.PlayAudio(AudioClipType.PlayerTakeDamage);
            }
            
            powerUpBallObtainedNetEvents.Clear();
        }
    }
}