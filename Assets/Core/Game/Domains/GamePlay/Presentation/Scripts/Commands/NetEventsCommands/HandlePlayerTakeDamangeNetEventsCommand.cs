using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
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

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
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
                _playerControllers.SetPlayerHealth(playerTakeDamageEvent.PlayerId, playerModel.Spaceship.Health.CurrentHealth, playerModel.Spaceship.Health.MaxHealth);
            }
            
            playerTakeDamageEvents.Clear();
        }
    }
}