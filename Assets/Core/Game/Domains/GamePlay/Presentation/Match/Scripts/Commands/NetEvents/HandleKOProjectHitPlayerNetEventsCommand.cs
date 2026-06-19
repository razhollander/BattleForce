using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleKOProjectHitPlayerNetEventsCommand : BaseCommand, ICommandVoid
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
            var events = _cachedPresentationEventsService.KOProjectHitPlayerNetEvents;
            if (events.Count == 0)
            {
                return;
            }

            foreach (var evt in events)
            {
                _audioService.PlayAudio(AudioClipType.KOHit);
            }
            
            _cachedPresentationEventsService.KOProjectHitPlayerNetEvents.Clear();
        }
    }
}
