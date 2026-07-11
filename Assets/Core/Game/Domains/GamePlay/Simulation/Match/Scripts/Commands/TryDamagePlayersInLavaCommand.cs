using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryDamagePlayersInLavaCommand: BaseCommand, ICommandVoid
    {
        private ICommandFactory _commandFactory;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        
        private int _processedTick;
        private PlayerHitCommand _playerHitCommand;

        public TryDamagePlayersInLavaCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _playerHitCommand = _commandFactory.CreateCommandVoid<PlayerHitCommand>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
        }

        public void Execute()
        {
            var playerIdsToDamage = _playersInLavaTrackerService.GetPlayerIdsToDamage();

            foreach (var playerId in playerIdsToDamage)
            {
                _playersInLavaTrackerService.TryResetPlayerTimePassedSinceLastDamageTaken(playerId);
                _playerHitCommand
                    .SetPlayerIdGotHit(playerId)
                    .SetWasHitByAnotherPlayer(false)
                    .SetProcessedTick(_processedTick)
                    .SetHitDamage(_gamePlayConfigService.GamePlayConfig.Lava.DamageAmount)
                    .Execute();
            }
        }
    }
}