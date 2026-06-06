using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.AudioService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateYearsOfPainTalentNetEventsCommand : BaseCommand, ICommandVoid
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
            if (_cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents)
            {
                _matchPlayerControllers.PlayerYearsOfPainForPlayer(netEvent.CasterPlayerId, netEvent.Direction);
                _audioService.PlayAudio(AudioClipType.YearsOfPainCast, AudioChannelType.Fx);
            }

            _cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents.Clear();
        }
    }
}
