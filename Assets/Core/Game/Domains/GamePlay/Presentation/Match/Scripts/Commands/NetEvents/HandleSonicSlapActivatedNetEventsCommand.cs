using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleSonicSlapActivatedNetEventsCommand : BaseCommand, ICommandVoid
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
            var netEvents = _cachedPresentationEventsService.SonicSlapActivatedNetEvents;
            if (netEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                _matchPlayerControllers.ShowPowerUpEffect(netEvent.CasterPlayerId);
                
                foreach (var affectedPlayerId in netEvent.AffectedPlayerIds.AsSpan())
                {
                    _matchPlayerControllers.PlaySonicSnapEffectForPlayer(affectedPlayerId);
                }
            }
            _audioService.PlayAudio(AudioClipType.PowerUpActivated);
            _audioService.PlayAudio(AudioClipType.SonicSlap);
            netEvents.Clear();
        }
    }
}
