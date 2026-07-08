using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleCreateSoulGhostNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ISoulGhostControllers _soulGhostControllers;
        private IMatchDataService _matchDataService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _soulGhostControllers = _diContainer.Resolve<ISoulGhostControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.CreateSoulGhostNetEvents;
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                var ghost = netEvent.SoulGhost;
                var teamId = _matchDataService.GetPlayerTeamId(ghost.PlayerCasterId);
                _soulGhostControllers.CreateSoulGhost(ghost.Id, ghost.PlayerCasterId, teamId, ghost.Position.ToUnityVector2(), ghost.Direction.ToUnityVector2());
            }

            _audioService.PlayAudio(AudioClipType.SoulCast); // play only once no matter how many events received
            _cachedPresentationEventsService.CreateSoulGhostNetEvents.Clear();
        }
    }
}
