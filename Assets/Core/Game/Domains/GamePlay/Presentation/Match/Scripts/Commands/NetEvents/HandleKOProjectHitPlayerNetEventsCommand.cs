using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleKOProjectHitPlayerNetEventsCommand : ICommand
    {
        private DiContainer _diContainer;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.KOProjectHitPlayerNetEvents;
            if (events.Count == 0) return;

            _cachedPresentationEventsService.KOProjectHitPlayerNetEvents.Clear();
        }
    }
}
