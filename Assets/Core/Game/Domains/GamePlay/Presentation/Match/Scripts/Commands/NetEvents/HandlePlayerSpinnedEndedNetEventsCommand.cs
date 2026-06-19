using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerSpinnedEndedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _playerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PlayerSpinnedEndedNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _playerControllers.SetPlayersSpinnedState(netEvent.PlayerId, false);
                _audioService.PlayAudio(AudioClipType.SpinnedEnded);
            }

            events.Clear();
        }
    }
}
