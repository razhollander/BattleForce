using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateYearsOfPainTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IAudioService _audioService;
        private IWorldCameraController _worldCameraController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents.Count == 0)
            {
                return;
            }

            var didHitAnyPlayer = false;
            foreach (var netEvent in _cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents)
            {
                _matchPlayerControllers.PlayerYearsOfPainForPlayer(netEvent.CasterPlayerId, netEvent.Direction);
                didHitAnyPlayer |= netEvent.HasHit;
            }

            if (didHitAnyPlayer)
            {
                _worldCameraController.ShakeCamera(3f,0.25f);
                _audioService.PlayAudio(AudioClipType.YearsOfPainHit);
            }

            _cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents.Clear();
        }
    }
}
