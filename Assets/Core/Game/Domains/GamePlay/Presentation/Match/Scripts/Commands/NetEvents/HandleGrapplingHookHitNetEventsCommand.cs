using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleGrapplingHookHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IGrapplingHookProjectilesControllers _hookProjectilesControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _hookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PlayerGrapplingHookHitNetEvents;
            if (events.Count == 0) return;

            foreach (var netEvent in events)
            {
                _hookProjectilesControllers.UpdateOnHit(netEvent.HookProjectileId);
            }

            _cachedPresentationEventsService.PlayerGrapplingHookHitNetEvents.Clear();
        }
    }
}
