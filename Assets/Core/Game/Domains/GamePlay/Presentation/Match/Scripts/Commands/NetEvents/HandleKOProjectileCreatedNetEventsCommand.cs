using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleKOProjectileCreatedNetEventsCommand : ICommand
    {
        private DiContainer _diContainer;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IKOProjectilesControllers _koProjectilesControllers;

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.CreateKOProjectileNetEvents;
            if (netEvents.Count == 0) return;

            _koProjectilesControllers.HandleCreateEvents(netEvents);
            _cachedPresentationEventsService.CreateKOProjectileNetEvents.Clear();
        }
    }
}
