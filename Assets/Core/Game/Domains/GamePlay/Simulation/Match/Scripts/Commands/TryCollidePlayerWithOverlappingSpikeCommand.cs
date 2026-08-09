using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryCollidePlayerWithOverlappingSpikeCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ICommandFactory _commandFactory;
        private CollidePlayerWithEnvironmentSpikeCommand _collidePlayerWithEnvironmentSpikeCommand;

        private ushort _playerId;
        private int _processedTick;

        public TryCollidePlayerWithOverlappingSpikeCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public TryCollidePlayerWithOverlappingSpikeCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _collidePlayerWithEnvironmentSpikeCommand = _commandFactory.CreateCommandVoid<CollidePlayerWithEnvironmentSpikeCommand>();
        }

        public void Execute()
        {
            var playerTransform = _matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship.Transform;

            if (!_physicsSimulator.CircleCastOnEnvironmentSpikes(playerTransform.Position, playerTransform.Radius, out var spikeBodyData))
            {
                return;
            }

            _collidePlayerWithEnvironmentSpikeCommand
                .SetPlayerId(_playerId)
                .SetSpikeId(spikeBodyData.Id)
                .SetProcessedTick(_processedTick)
                .Execute();
        }
    }
}
