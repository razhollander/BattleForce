using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Scripts.Extensions;
using Core.Scripts.Services.HapticsService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerTakeDamangeNetEventsCommand: BaseCommand, ICommandVoid
    {
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerUIControllers _matchPlayerUIControllers;
        private ICommandFactory _commandFactory;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
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
                var playerTakeDamageId = playerTakeDamageEvent.PlayerId;
                var playerModel = _matchDataService.GetPlayer(playerTakeDamageId);
                var currentHealth = playerModel.Spaceship.Health.CurrentHealth;
                var maxHealth = playerModel.Spaceship.Health.MaxHealth;
                _commandFactory.CreateCommandVoid<PlayHapticsForPlayerCommand>()
                    .SetPlayerId(playerTakeDamageId)
                    .SetHapticProfileType(HapticType.DamageTaken)
                    .Execute();
                _playerControllers.SetPlayerHealth(playerTakeDamageId, currentHealth, maxHealth);
                _matchPlayerUIControllers.SetPlayerHealth(playerTakeDamageId, currentHealth, maxHealth);
            }
            
            playerTakeDamageEvents.Clear();
        }
    }
}