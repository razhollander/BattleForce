using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerSpinnedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var playerSpinnedEvents = _cachedPresentationEventsService.PlayerSpinnedNetEvents;
            if (playerSpinnedEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var spinnedEvent in playerSpinnedEvents)
            {
                var player = _matchDataService.GetPlayer(spinnedEvent.PlayerId);
                if (player != null)
                {
                    player.SpinEndOnTick = spinnedEvent.SpinEndOnTick;
                }
            }

            playerSpinnedEvents.Clear();
        }
    }
}
