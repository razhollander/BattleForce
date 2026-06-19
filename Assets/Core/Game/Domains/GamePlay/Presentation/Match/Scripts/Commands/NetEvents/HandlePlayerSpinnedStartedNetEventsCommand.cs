using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using Core.Scripts.Services.HapticsService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerSpinnedStartedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _playerControllers;
        private ICommandFactory _commandFactory;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PlayerSpinnedStartedNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _commandFactory.CreateCommandVoid<PlayHapticsForPlayerCommand>()
                    .SetPlayerId(netEvent.PlayerId)
                    .SetHapticProfileType(HapticType.Spinned).Execute();
                _playerControllers.SetPlayersSpinnedState(netEvent.PlayerId, true);
                _audioService.PlayAudio(AudioClipType.SpinnedStarted);
            }

            events.Clear();
        }
    }
}
