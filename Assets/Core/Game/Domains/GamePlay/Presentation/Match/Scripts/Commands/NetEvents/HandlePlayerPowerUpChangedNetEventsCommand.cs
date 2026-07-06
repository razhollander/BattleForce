using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerPowerUpChangedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PlayerPowerUpChangedNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.PlayerPowerUpChangedNetEvents)
            {
                _matchPlayerControllers.SetPlayerCurrentPowerUp(netEvent.PlayerId, netEvent.PowerUp);
            }

            _cachedPresentationEventsService.PlayerPowerUpChangedNetEvents.Clear();
        }
    }
}
