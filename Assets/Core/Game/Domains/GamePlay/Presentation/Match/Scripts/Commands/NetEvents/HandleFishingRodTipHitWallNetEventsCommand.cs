using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleFishingRodTipHitWallNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.FishingRodTipHitWallNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            // The tip starts returning (position keeps syncing from the server). Hook point for future wall-hit VFX/SFX.
            _cachedPresentationEventsService.FishingRodTipHitWallNetEvents.Clear();
        }
    }
}
