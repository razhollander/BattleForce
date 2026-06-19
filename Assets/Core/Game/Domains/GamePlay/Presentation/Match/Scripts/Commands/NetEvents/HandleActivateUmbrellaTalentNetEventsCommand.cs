using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateUmbrellaTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.ActivateUmbrellaTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var evt in events)
            {
                _matchPlayerControllers.SetPlayerUmbrellaState(evt.CasterPlayerId, true);
                _audioService.PlayAudio(AudioClipType.UmbrellaCast);
            }

            _cachedPresentationEventsService.ActivateUmbrellaTalentNetEvents.Clear();
        }
    }
}
