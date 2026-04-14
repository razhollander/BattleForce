using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleGrapplingHookDeactivatedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IGrapplingHookProjectilesControllers _hookProjectilesControllers;

        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _hookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PlayerGrapplingHookDeactivatedNetEvents;
            if (events.IsNullOrEmpty()) return;

            foreach (var netEvent in events)
            {
                _hookProjectilesControllers.DestroyGrapplingHookProjectile(netEvent.HookProjectileId);
                _matchDataService.RemoveGrapplingHookProjectile(netEvent.HookProjectileId);
            }

            _cachedPresentationEventsService.PlayerGrapplingHookDeactivatedNetEvents.Clear();
        }
    }
}
