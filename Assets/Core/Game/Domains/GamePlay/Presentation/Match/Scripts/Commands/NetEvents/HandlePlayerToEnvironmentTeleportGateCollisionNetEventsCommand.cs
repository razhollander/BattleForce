using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.PlayerTeleportFX;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand : BaseCommand, ICommandVoid
    {
        private INetEventsDataService _netEventsDataService;
        private IMatchDataService _matchDataService;
        private MatchEnvironmentTeleportGateControllers _gateControllers;
        private PlayerTeleportFXController _fxController;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _gateControllers = _diContainer.Resolve<MatchEnvironmentTeleportGateControllers>();
            _fxController = _diContainer.Resolve<PlayerTeleportFXController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            foreach (var kvp in _netEventsDataService.PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer)
            {
                var playerId = kvp.Key;
                var events = kvp.Value;
                foreach (var evt in events.AsSpan())
                {
                    HandleEvent(evt, playerId);
                }
            }
        }

        private void HandleEvent(PlayerToEnvironmentTeleportGateCollisionNetEventS2C evt, ushort playerId)
        {
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
                 _playerControllers.SetPlayerTransform(playerId, evt.DestinationPoint.ToUnityVector2(), playerState.Spaceship.Transform.Direction.ToUnityVector2());
            }
        }
    }
}
