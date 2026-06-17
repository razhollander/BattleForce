using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleHeadbuttHitEnemyNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.HeadbuttHitEnemyNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            _cachedPresentationEventsService.HeadbuttHitEnemyNetEvents.Clear();
        }
    }
}
