using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleShuffleSwapPlayerPositionNetEventsCommand : BaseCommand, ICommandVoid
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
            var netEvents = _cachedPresentationEventsService.ShuffleSwapPlayerPositionNetEvents;
            if (netEvents.Count == 0)
                return;

            _audioService.PlayRandomAudio(AudioClipType.SingleShufflePowerUp1, AudioClipType.SingleShufflePowerUp2);

            netEvents.Clear();
        }
    }
}
