using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateFishingRodTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IFishingRodTipControllers _fishingRodTipControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DeactivateFishingRodTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _fishingRodTipControllers.DestroyFishingRodTip(netEvent.ProjectileId);
                _matchDataService.RemoveFishingRodTip(netEvent.ProjectileId);
            }

            _cachedPresentationEventsService.DeactivateFishingRodTalentNetEvents.Clear();
        }
    }
}
