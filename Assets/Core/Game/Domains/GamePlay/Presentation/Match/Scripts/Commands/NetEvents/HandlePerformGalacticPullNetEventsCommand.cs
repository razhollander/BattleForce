using Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePerformGalacticPullNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IGalacticPullStarEffectControllers _galacticPullStarEffectControllers;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _galacticPullStarEffectControllers = _diContainer.Resolve<IGalacticPullStarEffectControllers>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PerformGalacticPullNetEvents;
            if (events.Count == 0)
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _galacticPullStarEffectControllers.ShowStar(netEvent.FieldId, netEvent.CasterTeamId);
                _matchPlayerControllers.ShowPowerUpEffect(netEvent.CasterPlayerId);
            }

            _audioService.PlayAudio(AudioClipType.PowerUpActivated);
            events.Clear();
        }
    }
}
