using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateSoulTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ISoulGhostControllers _soulGhostControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _soulGhostControllers = _diContainer.Resolve<ISoulGhostControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DeactivateSoulTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            var didAnyTeleport = false;
            foreach (var netEvent in events)
            {
                if (netEvent.DidTeleport)
                {
                    _playerControllers.SetPlayerTransform(netEvent.CasterPlayerId, netEvent.TeleportPosition, netEvent.TeleportDirection);
                    didAnyTeleport = true;
                }

                _soulGhostControllers.DestroySoulGhost(netEvent.GhostId);
                _matchDataService.RemoveSoulGhost(netEvent.GhostId);
            }

            if (didAnyTeleport)
            {
                _audioService.PlayAudio(AudioClipType.SoulTeleport);
            }

            _cachedPresentationEventsService.DeactivateSoulTalentNetEvents.Clear();
        }
    }
}
