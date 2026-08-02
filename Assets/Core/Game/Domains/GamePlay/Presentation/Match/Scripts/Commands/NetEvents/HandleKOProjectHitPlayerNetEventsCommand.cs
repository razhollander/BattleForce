using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleKOProjectHitPlayerNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;
        private IWorldCameraController _worldCameraController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.KOProjectHitPlayerNetEvents;
            if (events.Count == 0)
            {
                return;
            }

            _worldCameraController.ShakeCamera(2f,0.25f);
            _audioService.PlayAudio(AudioClipType.KOHit);
            _cachedPresentationEventsService.KOProjectHitPlayerNetEvents.Clear();
        }
    }
}
