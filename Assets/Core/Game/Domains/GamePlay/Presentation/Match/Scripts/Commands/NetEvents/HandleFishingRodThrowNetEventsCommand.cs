using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleFishingRodThrowNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.FishingRodThrowNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            // The thrown enemy's spin/push is reflected through the state snapshot and the spin net-event.
            // The caught enemy's aim arrow is hidden by the deactivate event that always follows a throw.
            _audioService.PlayAudio(AudioClipType.FishingRodThrowEnemy); // play only once no matter how many events received

            _cachedPresentationEventsService.FishingRodThrowNetEvents.Clear();
        }
    }
}
