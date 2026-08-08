using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class CollidePlayerWithEnvironmentSpikeCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private ICommandFactory _commandFactory;
        private TryHitPlayerCommand _tryHitPlayerCommand;

        private ushort _playerId;
        private ushort _spikeId;
        private int _processedTick;

        public CollidePlayerWithEnvironmentSpikeCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public CollidePlayerWithEnvironmentSpikeCommand SetSpikeId(ushort spikeId)
        {
            _spikeId = spikeId;
            return this;
        }

        public CollidePlayerWithEnvironmentSpikeCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _tryHitPlayerCommand = _commandFactory.CreateCommandVoid<TryHitPlayerCommand>();
        }

        public void Execute()
        {
            if (!_matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship.IsAlive)
            {
                return;
            }

            var damage = _gamePlayConfigService.GamePlayConfig.EnvironmentSpikes.Damage;
            _tryHitPlayerCommand
                .SetPlayerIdGotHit(_playerId)
                .SetHitDamage(damage)
                .SetProcessedTick(_processedTick)
                .Execute();

            _netEventsDataService.AddEnvironmentSpikePlayerCollisionNetEvent(_processedTick, _spikeId, _playerId);
        }
    }
}
