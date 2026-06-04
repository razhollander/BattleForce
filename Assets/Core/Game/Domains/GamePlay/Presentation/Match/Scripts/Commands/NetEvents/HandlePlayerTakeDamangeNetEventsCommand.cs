using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerTakeDamangeNetEventsCommand: BaseCommand, ICommandVoid
    {
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerUIControllers _matchPlayerUIControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var playerTakeDamageEvents = _cachedPresentationEventsService.PlayerTakeDamageNetEvents;
            if (playerTakeDamageEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var playerTakeDamageEvent in playerTakeDamageEvents)
            {
                var playerModel = _matchDataService.GetPlayer(playerTakeDamageEvent.PlayerId);
                var currentHealth = playerModel.Spaceship.Health.CurrentHealth;
                var maxHealth = playerModel.Spaceship.Health.MaxHealth;
                _playerControllers.SetPlayerHealth(playerTakeDamageEvent.PlayerId, currentHealth, maxHealth);
                _matchPlayerUIControllers.SetPlayerHealth(playerTakeDamageEvent.PlayerId, currentHealth, maxHealth);
                _audioService.PlayAudio(AudioClipType.PlayerTakeDamage, AudioChannelType.Fx);
            }
            
            playerTakeDamageEvents.Clear();
        }
    }
}