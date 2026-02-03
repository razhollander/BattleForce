using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerDiedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerUIControllers _matchPlayerUIControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var playerDiedEvents = _cachedPresentationEventsService.PlayerDiedNetEvents;
            if (playerDiedEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerDiedEvent in playerDiedEvents)
            {
                var playerId = playerDiedEvent.PlayerId;
                _matchDataService.GetPlayer(playerId).Spaceship.Shoot.MaxCooldown = playerDiedEvent.PlayerMaxShootCooldown; 
                _matchPlayerUIControllers.HidePlayerHealthBar(playerDiedEvent.PlayerId);
            }

            playerDiedEvents.Clear();
        }
    }
}
