using Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateNukePowerUpNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private INukeShockwaveEffectController _nukeShockwaveEffectController;
        private IWorldCameraController _worldCameraController;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _nukeShockwaveEffectController = _diContainer.Resolve<INukeShockwaveEffectController>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.ActivateNukePowerUpNetEvents.Count == 0)
                return;

            foreach (var netEvent in _cachedPresentationEventsService.ActivateNukePowerUpNetEvents)
            {
                var position = netEvent.CasterPosition.ToUnityVector2();
                _nukeShockwaveEffectController.PlayEffect(position);
                _matchPlayerControllers.ShowPowerUpEffect(netEvent.CasterPlayerId);
            }

            _audioService.PlayAudio(AudioClipType.Nuke);
            _worldCameraController.ShakeCamera(15, 0.6f);
            _cachedPresentationEventsService.ActivateNukePowerUpNetEvents.Clear();
        }
    }
}
