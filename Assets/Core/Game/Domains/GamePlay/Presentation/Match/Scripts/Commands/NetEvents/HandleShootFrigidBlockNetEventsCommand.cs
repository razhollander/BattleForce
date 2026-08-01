using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleShootFrigidBlockNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IFrigidBlocksControllers _frigidBlocksControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _frigidBlocksControllers = _diContainer.Resolve<IFrigidBlocksControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.ShootFrigidBlockNetEvents;
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                var block = netEvent.FrigidBlock;
                _frigidBlocksControllers.CreateFrigidBlock(block.Id, block.Position.ToUnityVector2(), block.Rotation.ToUnityVector2());
            }

            _audioService.PlayAudio(AudioClipType.FrigidBlockActivated);

            _cachedPresentationEventsService.ShootFrigidBlockNetEvents.Clear();
        }
    }
}
