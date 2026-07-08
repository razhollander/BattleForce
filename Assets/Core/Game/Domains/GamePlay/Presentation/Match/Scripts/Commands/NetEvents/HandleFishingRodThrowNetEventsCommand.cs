using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleFishingRodThrowNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.FishingRodThrowNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            // The thrown enemy's spin/push is reflected through the state snapshot and the spin net-event.
            // Hook point for future throw VFX/SFX. The tip is removed by the deactivate event.
            _cachedPresentationEventsService.FishingRodThrowNetEvents.Clear();
        }
    }
}
