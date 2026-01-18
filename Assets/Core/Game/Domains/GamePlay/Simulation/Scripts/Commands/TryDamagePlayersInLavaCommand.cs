using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class TryDamagePlayersInLavaCommand: BaseCommand, ICommandVoid
    {
        private ICommandFactory _commandFactory;
        private SimulationGamePlayConfig _gamePlayConfig;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        
        private int _processedTick;
        private PlayerHitCommand _playerHitCommand;
        private NetworkConfig _networkConfig;

        public TryDamagePlayersInLavaCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _playerHitCommand = _commandFactory.CreateCommandVoid<PlayerHitCommand>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
        }

        public void Execute()
        {
            var playerIds = _playersInLavaTrackerService.StepAndGetPlayerIdsToDamage(_networkConfig.DeltaTime);

            foreach (var playerId in playerIds)
            {
                _playerHitCommand.SetPlayerId(playerId).SetProcessedTick(_processedTick).SetHitDamage(_gamePlayConfig.Lava.DamageAmount);
            }
        }
    }
}