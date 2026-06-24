using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IEnvironmentTeleportGateControllers _teleportGateControllers;
        private IPlayerTeleportEffectController _teelportEffectController;
        private IMatchPlayerControllers _playerControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _teleportGateControllers = _diContainer.Resolve<IEnvironmentTeleportGateControllers>();
            _teelportEffectController = _diContainer.Resolve<IPlayerTeleportEffectController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PlayerToEnvironmentTeleportGateCollisionNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var teleportGateCollisionEvent in _cachedPresentationEventsService.PlayerToEnvironmentTeleportGateCollisionNetEvents)
            {
                var playerId = teleportGateCollisionEvent.PlayerId;
                var teleportPairId = teleportGateCollisionEvent.TeleportGatePairId;
                _teleportGateControllers.PlayTeleportAnimation(teleportPairId);
                _teelportEffectController.PlayEffect(teleportGateCollisionEvent.EnterPoint.ToUnityVector2());
                _teelportEffectController.PlayEffect(teleportGateCollisionEvent.ExitPoint.ToUnityVector2());
                var playerState = _matchDataService.GetPlayer(playerId);
                _playerControllers.SetPlayerTransform(playerId, playerState.Spaceship.Transform.Position, playerState.Spaceship.Transform.Direction);
                _audioService.PlayAudio(AudioClipType.TeleportGateCollision);
            }
            
            _cachedPresentationEventsService.PlayerToEnvironmentTeleportGateCollisionNetEvents.Clear();
        }
    }
}
