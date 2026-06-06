using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.AudioService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleGrapplingHookHitWallNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IGrapplingHookProjectilesControllers _hookProjectilesControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _hookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.GrapplingHookHitWallNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _hookProjectilesControllers.UpdateOnHit(netEvent.ProjectileId);
                _audioService.PlayAudio(AudioClipType.GrapplingHookHit, AudioChannelType.Fx);
            }

            _cachedPresentationEventsService.GrapplingHookHitWallNetEvents.Clear();
        }
    }
}
