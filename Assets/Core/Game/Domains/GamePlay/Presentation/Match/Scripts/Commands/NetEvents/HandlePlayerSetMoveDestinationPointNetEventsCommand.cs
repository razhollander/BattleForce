using Core.Game.Domains.GamePlay.Presentation.Match.Features.MoveDestinationPointIndicator.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerSetMoveDestinationPointNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMoveDestinationPointIndicatorController _moveDestinationPointIndicatorController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moveDestinationPointIndicatorController = _diContainer.Resolve<IMoveDestinationPointIndicatorController>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PlayerSetMoveDestinationPointNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _moveDestinationPointIndicatorController.ShowIndicator(netEvent.DestinationPoint.ToUnityVector2());
            }

            events.Clear();
        }
    }
}
