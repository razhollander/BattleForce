using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleMoleSpawnedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMoleControllers _moleControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var moleSpawnedNetEvents = _cachedPresentationEventsService.MoleSpawnedNetEvents;

            if (moleSpawnedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleSpawnedNetEvent in moleSpawnedNetEvents)
            {
                _moleControllers.SetMoleOutsideHole(moleSpawnedNetEvent.MoleId, moleSpawnedNetEvent.Position.ToUnityVector2());
            }

            _audioService.PlayAudio(AudioClipType.MoleSpawned);
            moleSpawnedNetEvents.Clear();
        }
    }
}
