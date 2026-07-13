using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleFishingRodCaughtEnemyNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchDataService _matchDataService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.FishingRodCaughtEnemyNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            // The tip line follows the enemy via the synced projectile position. The phase is synced through this event
            // (not per-tick deltas), so mark the tip as caught here to start showing its throw-aim arrow.
            foreach (var caughtEvent in events)
            {
                var tip = _matchDataService.GetFishingRodTip(caughtEvent.ProjectileId);
                if (tip != null)
                {
                    tip.Phase = FishingRodTipPhase.CaughtEnemy;
                }
            }

            _audioService.StopLoopAudio(AudioClipType.FishingRodActivate);
            _audioService.PlayAudio(AudioClipType.FishingRodCatch); // play only once no matter how many events received

            _cachedPresentationEventsService.FishingRodCaughtEnemyNetEvents.Clear();
        }
    }
}
