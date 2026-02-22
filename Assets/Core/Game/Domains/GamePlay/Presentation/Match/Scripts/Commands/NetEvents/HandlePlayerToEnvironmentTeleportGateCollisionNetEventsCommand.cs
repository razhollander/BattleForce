using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.PlayerTeleportFX;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private MatchEnvironmentTeleportGateControllers _gateControllers;
        private PlayerTeleportFXController _fxController;
        private IMatchPlayerControllers _playerControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _gateControllers = _diContainer.Resolve<MatchEnvironmentTeleportGateControllers>();
            _fxController = _diContainer.Resolve<PlayerTeleportFXController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            foreach (var environmentTeleportGateCollisionNetEvent in _cachedPresentationEventsService.PlayerToEnvironmentTeleportGateCollisionNetEvents)
            {
               HandleEvent(environmentTeleportGateCollisionNetEvent);
            }
        }

        private void HandleEvent(PlayerToEnvironmentTeleportGateCollisionNetEventS2C evt)
        {
            var playerId = evt.PlayerId;
            // Identify gates
            var pairId = evt.TeleportGatePairId;
            var gateA = _gateControllers.GetGate(pairId, false);
            var gateB = _gateControllers.GetGate(pairId, true);

            if (gateA != null && gateB != null)
            {
                gateA.PlayAnimation();
                gateB.PlayAnimation();
            }

            // FX
            _fxController.PlayFX(evt.EnterPoint.ToUnityVector2());
            _fxController.PlayFX(evt.DestinationPoint.ToUnityVector2());

            // Snap Player Position
            var playerState = _matchDataService.GetPlayer(playerId);
            if (playerState != null)
            {
                 _playerControllers.SetPlayerTransform(playerId, evt.DestinationPoint, playerState.Spaceship.Transform.Direction);
            }
        }
    }
}
