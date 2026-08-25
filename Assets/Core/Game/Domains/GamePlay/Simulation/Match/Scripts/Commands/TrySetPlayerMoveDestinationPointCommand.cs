using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TrySetPlayerMoveDestinationPointCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPlayersMoveDestinationPointService _playersMoveDestinationPointService;
        private INetEventsDataService _netEventsDataService;

        private ushort _playerId;
        private Vector2 _destinationPoint;
        private long _clientId;
        private int _processedTick;
        private bool _shouldShowIndicator;

        public TrySetPlayerMoveDestinationPointCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public TrySetPlayerMoveDestinationPointCommand SetDestinationPoint(Vector2 destinationPoint)
        {
            _destinationPoint = destinationPoint;
            return this;
        }

        public TrySetPlayerMoveDestinationPointCommand SetClientId(long clientId)
        {
            _clientId = clientId;
            return this;
        }

        public TrySetPlayerMoveDestinationPointCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public TrySetPlayerMoveDestinationPointCommand ShouldShowIndicator(bool shouldShowIndicator)
        {
            _shouldShowIndicator = shouldShowIndicator;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersMoveDestinationPointService = _diContainer.Resolve<IPlayersMoveDestinationPointService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var playerSpaceship = _matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship;
            var isRotationBlockedByTalent = playerSpaceship.TalentsState.IsSelectedTalentBlockingRotation();
            if (isRotationBlockedByTalent)
            {
                return;
            }

            _playersMoveDestinationPointService.SetPlayerMoveDestinationPoint(_playerId, _destinationPoint, playerSpaceship.Transform.Direction);

            if (_shouldShowIndicator)
            {
                _netEventsDataService.AddPlayerSetMoveDestinationPointNetEvent(_processedTick, _playerId, _destinationPoint, _clientId);
            }
        }
    }
}
