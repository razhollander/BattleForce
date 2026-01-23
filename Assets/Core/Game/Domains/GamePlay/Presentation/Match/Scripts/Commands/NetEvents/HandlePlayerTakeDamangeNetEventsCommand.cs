using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class HandlePlayerTakeDamangeNetEventsCommand: BaseCommand, ICommandVoid
    {
        private IPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerUIControllers _matchPlayerUIControllers;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
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
            }
            
            playerTakeDamageEvents.Clear();
        }
    }
}