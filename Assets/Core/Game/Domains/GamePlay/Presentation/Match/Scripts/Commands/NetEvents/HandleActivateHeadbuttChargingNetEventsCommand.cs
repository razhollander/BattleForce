using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateHeadbuttChargingNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.ActivateHeadbuttChargingNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var evt in events)
            {
                _playerControllers.SetPlayerHeadbuttChargingState(evt.CasterPlayerId, true);
            }

            _cachedPresentationEventsService.ActivateHeadbuttChargingNetEvents.Clear();
        }
    }
}
