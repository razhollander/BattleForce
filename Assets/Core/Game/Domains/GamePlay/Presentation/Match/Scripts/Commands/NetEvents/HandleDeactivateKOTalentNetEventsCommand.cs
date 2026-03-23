using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateKOTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IKOProjectilesControllers _koProjectilesControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DeactivateKOTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                var koProjectileId = netEvent.KoProjectileId;
                _koProjectilesControllers.DestroyKOProjectile(koProjectileId);
            }

            _cachedPresentationEventsService.DeactivateKOTalentNetEvents.Clear();
        }
    }
}
