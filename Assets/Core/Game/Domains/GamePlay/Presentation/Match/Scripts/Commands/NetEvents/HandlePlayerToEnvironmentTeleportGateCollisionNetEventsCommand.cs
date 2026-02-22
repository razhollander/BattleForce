using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportFX;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private EnvironmentTeleportGateControllers _teleportGateControllers;
        private PlayerTeleportFXController _fxController;
        private IMatchPlayerControllers _playerControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _teleportGateControllers = _diContainer.Resolve<EnvironmentTeleportGateControllers>();
            _fxController = _diContainer.Resolve<PlayerTeleportFXController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
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
                _fxController.PlayFX(teleportGateCollisionEvent.EnterPoint.ToUnityVector2());
                _fxController.PlayFX(teleportGateCollisionEvent.ExitPoint.ToUnityVector2());
                var playerState = _matchDataService.GetPlayer(playerId);
                _playerControllers.SetPlayerTransform(playerId, playerState.Spaceship.Transform.Position, playerState.Spaceship.Transform.Direction);
            }
            
            _cachedPresentationEventsService.PlayerToEnvironmentTeleportGateCollisionNetEvents.Clear();
        }
    }
}
