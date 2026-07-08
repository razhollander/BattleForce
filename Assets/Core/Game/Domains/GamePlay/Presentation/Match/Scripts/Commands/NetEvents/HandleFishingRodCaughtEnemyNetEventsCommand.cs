using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleFishingRodCaughtEnemyNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.FishingRodCaughtEnemyNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            // The caught tip is pinned to the enemy via the synced projectile position, so the line follows automatically.
            // This command exists so the cached events are consumed each frame (hook point for future catch VFX/SFX).
            _cachedPresentationEventsService.FishingRodCaughtEnemyNetEvents.Clear();
        }
    }
}
